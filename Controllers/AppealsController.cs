using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ObrashcheniyaWeb.Data;
using ObrashcheniyaWeb.Models;
using ObrashcheniyaWeb.Models.ViewModels;

namespace ObrashcheniyaWeb.Controllers;

/// <summary>
/// Обращения — центральный модуль системы. Объединяет функции формы
/// пользователя (просмотр списка, создание, поиск) и формы администратора
/// (изменение статуса, обработка, удаление, фильтрация) из диплома.
/// </summary>
[Authorize]
public class AppealsController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<AppealsController> _logger;

    public AppealsController(AppDbContext db, ILogger<AppealsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>Текущий идентификатор сотрудника из claims.</summary>
    private int CurrentEmployeeId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? throw new InvalidOperationException("Не удалось определить сотрудника."));

    // ---------- Список обращений с фильтрацией и поиском ----------
    [HttpGet]
    public async Task<IActionResult> Index(AppealFilterViewModel filter)
    {
        var query = _db.Appeals
            .AsNoTracking()
            .Include(a => a.Citizen)
            .Include(a => a.RequestType)
            .Include(a => a.Status)
            .Include(a => a.Employee)
            .AsQueryable();

        // Поиск по ФИО гражданина (как в форме пользователя диплома).
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(a =>
                a.Citizen!.LastName.Contains(term) ||
                a.Citizen.FirstName.Contains(term) ||
                (a.Citizen.MiddleName != null && a.Citizen.MiddleName.Contains(term)));
        }

        // Фильтр по статусу, типу и диапазону дат (функции администратора).
        if (filter.StatusId.HasValue)
            query = query.Where(a => a.StatusId == filter.StatusId.Value);
        if (filter.RequestTypeId.HasValue)
            query = query.Where(a => a.RequestTypeId == filter.RequestTypeId.Value);
        if (filter.DateFrom.HasValue)
            query = query.Where(a => a.SubmissionDate >= filter.DateFrom.Value.Date);
        if (filter.DateTo.HasValue)
            query = query.Where(a => a.SubmissionDate <= filter.DateTo.Value.Date);

        filter.Appeals = await query
            .OrderByDescending(a => a.SubmissionDate)
            .ThenByDescending(a => a.Id)
            .ToListAsync();

        filter.Statuses = new SelectList(
            await _db.Statuses.AsNoTracking().OrderBy(s => s.Id).ToListAsync(),
            nameof(Status.Id), nameof(Status.Name), filter.StatusId);
        filter.RequestTypes = new SelectList(
            await _db.RequestTypes.AsNoTracking().OrderBy(t => t.Name).ToListAsync(),
            nameof(RequestType.Id), nameof(RequestType.Name), filter.RequestTypeId);

        return View(filter);
    }

    // ---------- Просмотр карточки обращения ----------
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var appeal = await _db.Appeals
            .AsNoTracking()
            .Include(a => a.Citizen)
            .Include(a => a.RequestType)
            .Include(a => a.Status)
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appeal is null) return NotFound();
        return View(appeal);
    }

    // ---------- Создание обращения (доступно пользователю и админу) ----------
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new CreateAppealViewModel();
        await PopulateCreateListsAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAppealViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCreateListsAsync(model);
            return View(model);
        }

        // Бизнес-логика из Листинга 4: новый обращение получает статус
        // «Зарегистрировано» (ID=1), дата поступления — текущая (DEFAULT GETDATE()),
        // ответственный — текущий сотрудник.
        var appeal = new Appeal
        {
            CitizenId = model.CitizenId,
            RequestTypeId = model.RequestTypeId,
            StatusId = StatusIds.Registered,
            EmployeeId = CurrentEmployeeId,
            SubmissionDate = DateTime.Today,
            Description = model.Description.Trim()
        };

        _db.Appeals.Add(appeal);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Зарегистрировано обращение №{Id} сотрудником {EmployeeId}.",
            appeal.Id, CurrentEmployeeId);
        TempData["Success"] = "Обращение зарегистрировано.";
        return RedirectToAction(nameof(Index));
    }

    // ---------- Изменение статуса (только администратор) ----------
    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> ChangeStatus(int id)
    {
        var appeal = await _db.Appeals
            .Include(a => a.Citizen)
            .Include(a => a.Status)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appeal is null) return NotFound();

        var model = new ChangeStatusViewModel
        {
            AppealId = appeal.Id,
            CitizenName = appeal.Citizen!.ShortName,
            CurrentStatus = appeal.Status!.Name,
            StatusId = appeal.StatusId,
            CompletionDate = appeal.CompletionDate,
            Result = appeal.Result
        };
        await PopulateStatusListAsync(model);
        return View(model);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(ChangeStatusViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateStatusListAsync(model);
            return View(model);
        }

        var appeal = await _db.Appeals.FirstOrDefaultAsync(a => a.Id == model.AppealId);
        if (appeal is null) return NotFound();

        // Аналог Листинга 5: обновление статуса, даты исполнения и результата.
        appeal.StatusId = model.StatusId;
        appeal.CompletionDate = model.CompletionDate;
        appeal.Result = string.IsNullOrWhiteSpace(model.Result) ? null : model.Result.Trim();

        await _db.SaveChangesAsync();
        _logger.LogInformation("Обращение №{Id}: статус изменён на {StatusId}.",
            appeal.Id, model.StatusId);
        TempData["Success"] = "Статус обращения обновлён.";
        return RedirectToAction(nameof(Index));
    }

    // ---------- Обработка: назначение ответственного + результат (админ) ----------
    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Process(int id)
    {
        var appeal = await _db.Appeals
            .Include(a => a.Citizen)
            .Include(a => a.RequestType)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appeal is null) return NotFound();

        var model = new ProcessAppealViewModel
        {
            AppealId = appeal.Id,
            CitizenName = appeal.Citizen!.ShortName,
            RequestTypeName = appeal.RequestType!.Name,
            Description = appeal.Description,
            EmployeeId = appeal.EmployeeId,
            StatusId = appeal.StatusId,
            CompletionDate = appeal.CompletionDate,
            Result = appeal.Result
        };
        await PopulateProcessListsAsync(model);
        return View(model);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Process(ProcessAppealViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateProcessListsAsync(model);
            return View(model);
        }

        var appeal = await _db.Appeals.FirstOrDefaultAsync(a => a.Id == model.AppealId);
        if (appeal is null) return NotFound();

        appeal.EmployeeId = model.EmployeeId;
        appeal.StatusId = model.StatusId;
        appeal.CompletionDate = model.CompletionDate;
        appeal.Result = string.IsNullOrWhiteSpace(model.Result) ? null : model.Result.Trim();

        await _db.SaveChangesAsync();
        _logger.LogInformation("Обращение №{Id} обработано: ответственный {EmployeeId}, статус {StatusId}.",
            appeal.Id, model.EmployeeId, model.StatusId);
        TempData["Success"] = "Обращение обработано.";
        return RedirectToAction(nameof(Index));
    }

    // ---------- Удаление обращения (только администратор) ----------
    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        var appeal = await _db.Appeals
            .AsNoTracking()
            .Include(a => a.Citizen)
            .Include(a => a.RequestType)
            .Include(a => a.Status)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appeal is null) return NotFound();
        return View(appeal);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Policy = "AdminOnly")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var appeal = await _db.Appeals.FirstOrDefaultAsync(a => a.Id == id);
        if (appeal is null) return NotFound();

        _db.Appeals.Remove(appeal);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Обращение №{Id} удалено.", id);
        TempData["Success"] = "Обращение удалено.";
        return RedirectToAction(nameof(Index));
    }

    // ---------- Вспомогательные методы заполнения списков ----------
    private async Task PopulateCreateListsAsync(CreateAppealViewModel model)
    {
        var citizens = await _db.Citizens
            .AsNoTracking()
            .OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
            .ToListAsync();

        model.Citizens = new SelectList(
            citizens.Select(c => new { c.Id, Display = c.FullName }),
            "Id", "Display", model.CitizenId);

        model.RequestTypes = new SelectList(
            await _db.RequestTypes.AsNoTracking().OrderBy(t => t.Name).ToListAsync(),
            nameof(RequestType.Id), nameof(RequestType.Name), model.RequestTypeId);
    }

    private async Task PopulateStatusListAsync(ChangeStatusViewModel model)
    {
        model.Statuses = new SelectList(
            await _db.Statuses.AsNoTracking().OrderBy(s => s.Id).ToListAsync(),
            nameof(Status.Id), nameof(Status.Name), model.StatusId);
    }

    private async Task PopulateProcessListsAsync(ProcessAppealViewModel model)
    {
        model.Employees = new SelectList(
            await _db.Employees.AsNoTracking().OrderBy(e => e.FullName).ToListAsync(),
            nameof(Employee.Id), nameof(Employee.FullName), model.EmployeeId);
        model.Statuses = new SelectList(
            await _db.Statuses.AsNoTracking().OrderBy(s => s.Id).ToListAsync(),
            nameof(Status.Id), nameof(Status.Name), model.StatusId);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObrashcheniyaWeb.Data;
using ObrashcheniyaWeb.Models;

namespace ObrashcheniyaWeb.Controllers;

/// <summary>
/// Управление справочником статусов (вкладка «Статусы» формы администратора).
/// Удаление запрещено при наличии обращений с данным статусом.
/// </summary>
[Authorize(Policy = "AdminOnly")]
public class StatusesController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<StatusesController> _logger;

    public StatusesController(AppDbContext db, ILogger<StatusesController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var statuses = await _db.Statuses
            .AsNoTracking()
            .Select(s => new { Status = s, AppealCount = s.Appeals.Count() })
            .OrderBy(x => x.Status.Id)
            .ToListAsync();

        ViewBag.AppealCounts = statuses.ToDictionary(x => x.Status.Id, x => x.AppealCount);
        return View(statuses.Select(x => x.Status).ToList());
    }

    [HttpGet]
    public IActionResult Create() => View(new Status());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Status model)
    {
        if (!ModelState.IsValid) return View(model);

        _db.Statuses.Add(model);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Добавлен статус {Name} (ID {Id}).", model.Name, model.Id);
        TempData["Success"] = "Статус добавлен.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var status = await _db.Statuses.FindAsync(id);
        if (status is null) return NotFound();
        return View(status);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Status model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        var status = await _db.Statuses.FindAsync(id);
        if (status is null) return NotFound();

        status.Name = model.Name;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Изменён статус ID {Id}.", id);
        TempData["Success"] = "Статус обновлён.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var status = await _db.Statuses
            .AsNoTracking()
            .Include(s => s.Appeals)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (status is null) return NotFound();
        ViewBag.HasAppeals = status.Appeals.Any();
        return View(status);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var status = await _db.Statuses
            .Include(s => s.Appeals)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (status is null) return NotFound();

        if (status.Appeals.Any())
        {
            TempData["Error"] = "Невозможно удалить статус: есть обращения с этим статусом.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            _db.Statuses.Remove(status);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Удалён статус ID {Id}.", id);
            TempData["Success"] = "Статус удалён.";
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Ошибка удаления статуса ID {Id}.", id);
            TempData["Error"] = "Невозможно удалить статус: нарушается целостность данных.";
        }

        return RedirectToAction(nameof(Index));
    }
}

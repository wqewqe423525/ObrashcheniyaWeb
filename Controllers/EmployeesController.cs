using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ObrashcheniyaWeb.Data;
using ObrashcheniyaWeb.Models;
using ObrashcheniyaWeb.Security;
using ObrashcheniyaWeb.Models.ViewModels;

namespace ObrashcheniyaWeb.Controllers;

/// <summary>
/// Управление учётными записями сотрудников (вкладка «Сотрудники» формы
/// администратора). Пароли хэшируются (PBKDF2). Логин уникален.
/// </summary>
[Authorize(Policy = "AdminOnly")]
public class EmployeesController : Controller
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(AppDbContext db, IPasswordHasher hasher, ILogger<EmployeesController> logger)
    {
        _db = db;
        _hasher = hasher;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var employees = await _db.Employees
            .AsNoTracking()
            .OrderBy(e => e.FullName)
            .ToListAsync();
        return View(employees);
    }

    [HttpGet]
    public IActionResult Create()
    {
        PopulateRoles();
        return View(new EmployeeFormModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeFormModel model)
    {
        // При создании пароль обязателен.
        if (string.IsNullOrWhiteSpace(model.Password))
            ModelState.AddModelError(nameof(model.Password), "Введите пароль.");

        await ValidateUniqueLoginAsync(model.Login, null);

        if (!ModelState.IsValid)
        {
            PopulateRoles();
            return View(model);
        }

        var employee = new Employee
        {
            FullName = model.FullName.Trim(),
            Position = model.Position?.Trim(),
            Login = model.Login.Trim(),
            Role = model.Role,
            Password = _hasher.Hash(model.Password!.Trim())
        };

        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Создана учётная запись {Login} ({Role}).", employee.Login, employee.Role);
        TempData["Success"] = "Сотрудник добавлен.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var employee = await _db.Employees.FindAsync(id);
        if (employee is null) return NotFound();

        PopulateRoles();
        var model = new EmployeeFormModel
        {
            Id = employee.Id,
            FullName = employee.FullName,
            Position = employee.Position,
            Login = employee.Login,
            Role = employee.Role
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EmployeeFormModel model)
    {
        if (id != model.Id) return BadRequest();

        await ValidateUniqueLoginAsync(model.Login, id);

        if (!ModelState.IsValid)
        {
            PopulateRoles();
            return View(model);
        }

        var employee = await _db.Employees.FindAsync(id);
        if (employee is null) return NotFound();

        employee.FullName = model.FullName.Trim();
        employee.Position = model.Position?.Trim();
        employee.Login = model.Login.Trim();
        employee.Role = model.Role;

        // Пароль обновляется только если введён новый.
        if (!string.IsNullOrWhiteSpace(model.Password))
            employee.Password = _hasher.Hash(model.Password.Trim());

        await _db.SaveChangesAsync();
        _logger.LogInformation("Изменена учётная запись ID {Id}.", id);
        TempData["Success"] = "Данные сотрудника обновлены.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var employee = await _db.Employees
            .AsNoTracking()
            .Include(e => e.Appeals)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee is null) return NotFound();
        ViewBag.HasAppeals = employee.Appeals.Any();
        return View(employee);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        // Нельзя удалить самого себя.
        var currentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (currentId == id)
        {
            TempData["Error"] = "Нельзя удалить собственную учётную запись.";
            return RedirectToAction(nameof(Index));
        }

        var employee = await _db.Employees
            .Include(e => e.Appeals)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee is null) return NotFound();

        if (employee.Appeals.Any())
        {
            TempData["Error"] = "Невозможно удалить сотрудника: за ним закреплены обращения.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            _db.Employees.Remove(employee);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Удалена учётная запись ID {Id}.", id);
            TempData["Success"] = "Сотрудник удалён.";
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Ошибка удаления сотрудника ID {Id}.", id);
            TempData["Error"] = "Невозможно удалить сотрудника: нарушается целостность данных.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateUniqueLoginAsync(string? login, int? excludeId)
    {
        if (string.IsNullOrWhiteSpace(login)) return;
        var trimmed = login.Trim();
        var exists = await _db.Employees
            .AnyAsync(e => e.Login == trimmed && (!excludeId.HasValue || e.Id != excludeId.Value));
        if (exists)
            ModelState.AddModelError(nameof(EmployeeFormModel.Login), "Такой логин уже используется.");
    }

    private void PopulateRoles()
    {
        ViewBag.Roles = new SelectList(Roles.All);
    }
}

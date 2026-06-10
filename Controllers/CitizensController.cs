using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObrashcheniyaWeb.Data;
using ObrashcheniyaWeb.Models;

namespace ObrashcheniyaWeb.Controllers;

/// <summary>
/// Управление гражданами (вкладка «Граждане» формы администратора).
/// Удаление возможно только при отсутствии связанных обращений.
/// </summary>
[Authorize(Policy = "AdminOnly")]
public class CitizensController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<CitizensController> _logger;

    public CitizensController(AppDbContext db, ILogger<CitizensController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search)
    {
        var query = _db.Citizens.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c =>
                c.LastName.Contains(term) ||
                c.FirstName.Contains(term) ||
                (c.MiddleName != null && c.MiddleName.Contains(term)) ||
                (c.Phone != null && c.Phone.Contains(term)));
        }

        ViewBag.Search = search;
        var list = await query.OrderBy(c => c.LastName).ThenBy(c => c.FirstName).ToListAsync();
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var citizen = await _db.Citizens
            .AsNoTracking()
            .Include(c => c.Appeals).ThenInclude(a => a.RequestType)
            .Include(c => c.Appeals).ThenInclude(a => a.Status)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (citizen is null) return NotFound();
        return View(citizen);
    }

    [HttpGet]
    public IActionResult Create() => View(new Citizen());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Citizen model)
    {
        if (!ModelState.IsValid) return View(model);

        _db.Citizens.Add(model);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Добавлен гражданин {Name} (ID {Id}).", model.FullName, model.Id);
        TempData["Success"] = "Гражданин добавлен.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var citizen = await _db.Citizens.FindAsync(id);
        if (citizen is null) return NotFound();
        return View(citizen);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Citizen model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        var citizen = await _db.Citizens.FindAsync(id);
        if (citizen is null) return NotFound();

        citizen.LastName = model.LastName;
        citizen.FirstName = model.FirstName;
        citizen.MiddleName = model.MiddleName;
        citizen.BirthDate = model.BirthDate;
        citizen.Phone = model.Phone;
        citizen.Email = model.Email;
        citizen.Address = model.Address;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Изменён гражданин ID {Id}.", id);
        TempData["Success"] = "Данные гражданина обновлены.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var citizen = await _db.Citizens
            .AsNoTracking()
            .Include(c => c.Appeals)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (citizen is null) return NotFound();
        ViewBag.HasAppeals = citizen.Appeals.Any();
        return View(citizen);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var citizen = await _db.Citizens
            .Include(c => c.Appeals)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (citizen is null) return NotFound();

        // Проверка ссылочной целостности на уровне приложения (как в дипломе).
        if (citizen.Appeals.Any())
        {
            TempData["Error"] = "Невозможно удалить гражданина: у него есть связанные обращения.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            _db.Citizens.Remove(citizen);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Удалён гражданин ID {Id}.", id);
            TempData["Success"] = "Гражданин удалён.";
        }
        catch (DbUpdateException ex)
        {
            // Подстраховка на уровне СУБД (ограничение FOREIGN KEY).
            _logger.LogWarning(ex, "Ошибка удаления гражданина ID {Id}.", id);
            TempData["Error"] = "Невозможно удалить гражданина: нарушается целостность данных.";
        }

        return RedirectToAction(nameof(Index));
    }
}

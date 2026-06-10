using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObrashcheniyaWeb.Data;
using ObrashcheniyaWeb.Models;

namespace ObrashcheniyaWeb.Controllers;

/// <summary>
/// Управление справочником типов обращений (вкладка «Типы обращений»
/// формы администратора). Удаление запрещено при наличии привязанных обращений.
/// </summary>
[Authorize(Policy = "AdminOnly")]
public class RequestTypesController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<RequestTypesController> _logger;

    public RequestTypesController(AppDbContext db, ILogger<RequestTypesController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var types = await _db.RequestTypes
            .AsNoTracking()
            .Select(t => new
            {
                Type = t,
                AppealCount = t.Appeals.Count()
            })
            .OrderBy(x => x.Type.Name)
            .ToListAsync();

        ViewBag.AppealCounts = types.ToDictionary(x => x.Type.Id, x => x.AppealCount);
        return View(types.Select(x => x.Type).ToList());
    }

    [HttpGet]
    public IActionResult Create() => View(new RequestType());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RequestType model)
    {
        if (!ModelState.IsValid) return View(model);

        _db.RequestTypes.Add(model);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Добавлен тип обращения {Name} (ID {Id}).", model.Name, model.Id);
        TempData["Success"] = "Тип обращения добавлен.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var type = await _db.RequestTypes.FindAsync(id);
        if (type is null) return NotFound();
        return View(type);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, RequestType model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        var type = await _db.RequestTypes.FindAsync(id);
        if (type is null) return NotFound();

        type.Name = model.Name;
        type.Description = model.Description;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Изменён тип обращения ID {Id}.", id);
        TempData["Success"] = "Тип обращения обновлён.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var type = await _db.RequestTypes
            .AsNoTracking()
            .Include(t => t.Appeals)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (type is null) return NotFound();
        ViewBag.HasAppeals = type.Appeals.Any();
        return View(type);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var type = await _db.RequestTypes
            .Include(t => t.Appeals)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (type is null) return NotFound();

        if (type.Appeals.Any())
        {
            TempData["Error"] = "Невозможно удалить тип: к нему привязаны обращения.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            _db.RequestTypes.Remove(type);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Удалён тип обращения ID {Id}.", id);
            TempData["Success"] = "Тип обращения удалён.";
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Ошибка удаления типа обращения ID {Id}.", id);
            TempData["Error"] = "Невозможно удалить тип: нарушается целостность данных.";
        }

        return RedirectToAction(nameof(Index));
    }
}

using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObrashcheniyaWeb.Data;
using ObrashcheniyaWeb.Models;
using ObrashcheniyaWeb.Models.ViewModels;

namespace ObrashcheniyaWeb.Controllers;

/// <summary>
/// Главная страница — Dashboard со статистикой по обращениям
/// (реализует пункт А4 «просмотр аналитики по обращениям» из диплома),
/// а также страница ошибок.
/// </summary>
public class HomeController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<HomeController> _logger;

    public HomeController(AppDbContext db, ILogger<HomeController> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var appeals = _db.Appeals.AsNoTracking();

        var model = new DashboardViewModel
        {
            TotalAppeals = await appeals.CountAsync(),
            TotalCitizens = await _db.Citizens.CountAsync(),
            TotalEmployees = await _db.Employees.CountAsync(),
            RegisteredCount = await appeals.CountAsync(a => a.StatusId == StatusIds.Registered),
            InProgressCount = await appeals.CountAsync(a => a.StatusId == StatusIds.InProgress),
            CompletedCount = await appeals.CountAsync(a => a.StatusId == StatusIds.Completed),
            RejectedCount = await appeals.CountAsync(a => a.StatusId == StatusIds.Rejected)
        };

        model.ByType = await _db.Appeals
            .AsNoTracking()
            .GroupBy(a => a.RequestType!.Name)
            .Select(g => new { g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Select(x => new ValueTuple<string, int>(x.Key, x.Count))
            .ToListAsync();

        model.ByEmployee = await _db.Appeals
            .AsNoTracking()
            .GroupBy(a => a.Employee!.FullName)
            .Select(g => new { g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Select(x => new ValueTuple<string, int>(x.Key, x.Count))
            .ToListAsync();

        model.RecentAppeals = await _db.Appeals
            .AsNoTracking()
            .Include(a => a.Citizen)
            .Include(a => a.RequestType)
            .Include(a => a.Status)
            .OrderByDescending(a => a.SubmissionDate)
            .ThenByDescending(a => a.Id)
            .Take(5)
            .ToListAsync();

        return View(model);
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? statusCode = null)
    {
        var model = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            StatusCode = statusCode ?? 500
        };

        model.Message = statusCode switch
        {
            404 => "Запрашиваемая страница не найдена.",
            403 => "Доступ к ресурсу запрещён.",
            _ => "Произошла внутренняя ошибка при обработке запроса."
        };

        if (statusCode is null)
            _logger.LogError("Отображена страница ошибки. RequestId={RequestId}", model.RequestId);

        return View(model);
    }
}

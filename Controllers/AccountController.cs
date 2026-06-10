using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObrashcheniyaWeb.Data;
using ObrashcheniyaWeb.Models;
using ObrashcheniyaWeb.Models.ViewModels;
using ObrashcheniyaWeb.Security;

namespace ObrashcheniyaWeb.Controllers;

/// <summary>
/// Авторизация в системе (форма входа из диплома, Рисунок 9).
/// Проверяет логин/пароль по таблице «Сотрудники», определяет роль и
/// перенаправляет в соответствующий интерфейс. EF Core заменяет прямой
/// SQL-запрос из Листинга 2; пароли проверяются по PBKDF2-хэшу.
/// </summary>
[AllowAnonymous]
public class AccountController : Controller
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly ILogger<AccountController> _logger;

    public AccountController(AppDbContext db, IPasswordHasher hasher, ILogger<AccountController> logger)
    {
        _db = db;
        _hasher = hasher;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        // Если уже вошёл — на главную.
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        // Проверка пустых полей (аналог проверки из Листинга 2 диплома).
        if (!ModelState.IsValid)
            return View(model);

        var login = model.Login.Trim();
        var employee = await _db.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Login == login);

        if (employee is null || !_hasher.Verify(model.Password.Trim(), employee.Password))
        {
            _logger.LogWarning("Неудачная попытка входа с логином {Login}.", login);
            ModelState.AddModelError(string.Empty, "Неверный логин или пароль.");
            return View(model);
        }

        // Формируем claims, в том числе роль (определяет доступный функционал).
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, employee.Id.ToString()),
            new(ClaimTypes.Name, employee.FullName),
            new(ClaimTypes.Role, employee.Role),
            new("Login", employee.Login)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        _logger.LogInformation("Сотрудник {Name} ({Role}) вошёл в систему.",
            employee.FullName, employee.Role);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var name = User.Identity?.Name;
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _logger.LogInformation("Сотрудник {Name} вышел из системы.", name);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();
}

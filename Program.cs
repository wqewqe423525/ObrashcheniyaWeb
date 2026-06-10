using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ObrashcheniyaWeb.Data;
using ObrashcheniyaWeb.Models;
using ObrashcheniyaWeb.Security;
using Serilog;

// Bootstrap-логгер до построения хоста — чтобы зафиксировать ошибки старта.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Запуск приложения «Система учёта обращений».");

    var builder = WebApplication.CreateBuilder(args);

    // ---------- Serilog ----------
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // ---------- EF Core (SQL Server) ----------
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Не задана строка подключения 'DefaultConnection'.");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

    // ---------- Сервисы приложения ----------
    builder.Services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();

    // ---------- Аутентификация по cookie + роли ----------
    builder.Services
        .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
            options.Cookie.HttpOnly = true;
            options.Cookie.Name = "ObrashcheniyaAuth";
        });

    builder.Services.AddAuthorization(options =>
    {
        // Политика «только администратор» — для разделов управления.
        options.AddPolicy("AdminOnly", policy => policy.RequireRole(Roles.Admin));
        // По умолчанию весь сайт требует аутентификации.
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
    });

    builder.Services.AddControllersWithViews();

    var app = builder.Build();

    // ---------- Конвейер обработки запросов ----------
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        // Глобальная обработка ошибок: исключения -> /Home/Error.
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    // Обработка HTTP-кодов ошибок (404 и т.п.) дружелюбной страницей.
    app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    // Логирование каждого запроса.
    app.UseSerilogRequestLogging();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    // ---------- Миграции + Seed данных ----------
    await DbInitializer.InitializeAsync(app.Services);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Приложение аварийно завершилось при запуске.");
}
finally
{
    Log.CloseAndFlush();
}

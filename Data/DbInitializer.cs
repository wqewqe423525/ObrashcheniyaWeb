using Microsoft.EntityFrameworkCore;
using ObrashcheniyaWeb.Models;
using ObrashcheniyaWeb.Security;

namespace ObrashcheniyaWeb.Data;

/// <summary>
/// Применяет миграции EF Core и заполняет базу тестовыми данными
/// в точности из Приложения А диплома (справочники + граждане + сотрудники +
/// обращения). Пароли сотрудников хэшируются при загрузке.
/// </summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("DbInitializer");

        logger.LogInformation("Применение миграций базы данных...");
        await context.Database.MigrateAsync();

        await SeedStatusesAsync(context, logger);
        await SeedRequestTypesAsync(context, logger);
        await SeedEmployeesAsync(context, hasher, logger);
        await SeedCitizensAsync(context, logger);
        await SeedAppealsAsync(context, logger);

        logger.LogInformation("Инициализация базы данных завершена.");
    }

    private static async Task SeedStatusesAsync(AppDbContext context, ILogger logger)
    {
        if (await context.Statuses.AnyAsync()) return;

        logger.LogInformation("Заполнение справочника статусов.");
        // Порядок соответствует INSERT INTO Статусы из диплома (ID 1..4).
        context.Statuses.AddRange(
            new Status { Name = "Зарегистрировано" },
            new Status { Name = "В работе" },
            new Status { Name = "Исполнено" },
            new Status { Name = "Отказано" });
        await context.SaveChangesAsync();
    }

    private static async Task SeedRequestTypesAsync(AppDbContext context, ILogger logger)
    {
        if (await context.RequestTypes.AnyAsync()) return;

        logger.LogInformation("Заполнение справочника типов обращений.");
        context.RequestTypes.AddRange(
            new RequestType { Name = "Поиск работы", Description = "Содействие в подборе подходящей вакансии" },
            new RequestType { Name = "Регистрация безработного", Description = "Постановка гражданина на учёт" },
            new RequestType { Name = "Консультация", Description = "Разъяснение норм трудового законодательства" },
            new RequestType { Name = "Профессиональная переподготовка", Description = "Направление на обучающие курсы" },
            new RequestType { Name = "Выплата пособия", Description = "Вопросы начисления и получения пособий" });
        await context.SaveChangesAsync();
    }

    private static async Task SeedEmployeesAsync(AppDbContext context, IPasswordHasher hasher, ILogger logger)
    {
        if (await context.Employees.AnyAsync()) return;

        logger.LogInformation("Заполнение учётных записей сотрудников.");
        // Логины/пароли/роли — из тестовых данных диплома; пароли хэшируются.
        context.Employees.AddRange(
            new Employee
            {
                FullName = "Кузнецова А.С.", Position = "Специалист I категории",
                Login = "staff1", Password = hasher.Hash("123"), Role = Roles.User
            },
            new Employee
            {
                FullName = "Орлова Т.В.", Position = "Специалист II категории",
                Login = "staff2", Password = hasher.Hash("123"), Role = Roles.User
            },
            new Employee
            {
                FullName = "Губарев А.Е.", Position = "Ведущий специалист",
                Login = "admin", Password = hasher.Hash("admin123"), Role = Roles.Admin
            });
        await context.SaveChangesAsync();
    }

    private static async Task SeedCitizensAsync(AppDbContext context, ILogger logger)
    {
        if (await context.Citizens.AnyAsync()) return;

        logger.LogInformation("Заполнение тестовых граждан.");
        context.Citizens.AddRange(
            new Citizen
            {
                LastName = "Иванов", FirstName = "Иван", MiddleName = "Петрович",
                BirthDate = new DateTime(1985, 3, 15), Phone = "89261234567",
                Email = "ivanov@mail.ru", Address = "г. Ставрополь, ул. Мира, 12"
            },
            new Citizen
            {
                LastName = "Петрова", FirstName = "Мария", MiddleName = "Ивановна",
                BirthDate = new DateTime(1990, 7, 22), Phone = "89164567890",
                Email = "petrova@mail.ru", Address = "г. Ставрополь, пр. Октябрьский, 5"
            },
            new Citizen
            {
                LastName = "Сидоров", FirstName = "Алексей", MiddleName = "Николаевич",
                BirthDate = new DateTime(1978, 11, 5), Phone = "89037654321",
                Email = "sidorov@mail.ru", Address = "г. Михайловск, ул. Ленина, 34"
            },
            new Citizen
            {
                LastName = "Козлова", FirstName = "Елена", MiddleName = "Сергеевна",
                BirthDate = new DateTime(1995, 2, 18), Phone = "89285551234",
                Email = "kozlova@mail.ru", Address = "г. Ставрополь, ул. Дзержинского, 7"
            },
            new Citizen
            {
                LastName = "Морозов", FirstName = "Дмитрий", MiddleName = null,
                BirthDate = new DateTime(1982, 9, 30), Phone = "89614443322",
                Email = null, Address = "г. Ставрополь, ул. Голенева, 19"
            });
        await context.SaveChangesAsync();
    }

    private static async Task SeedAppealsAsync(AppDbContext context, ILogger logger)
    {
        if (await context.Appeals.AnyAsync()) return;

        logger.LogInformation("Заполнение тестовых обращений.");
        // Соответствует INSERT INTO Обращения из диплома (ID граждан/типов/статусов 1-based).
        context.Appeals.AddRange(
            new Appeal
            {
                CitizenId = 1, RequestTypeId = 1, StatusId = StatusIds.Registered, EmployeeId = 1,
                SubmissionDate = new DateTime(2026, 4, 1), CompletionDate = null,
                Description = "Ищу работу бухгалтером, стаж 10 лет", Result = null
            },
            new Appeal
            {
                CitizenId = 2, RequestTypeId = 2, StatusId = StatusIds.InProgress, EmployeeId = 1,
                SubmissionDate = new DateTime(2026, 4, 3), CompletionDate = null,
                Description = "Сокращена с предприятия, необходима постановка на учёт", Result = null
            },
            new Appeal
            {
                CitizenId = 3, RequestTypeId = 3, StatusId = StatusIds.Completed, EmployeeId = 2,
                SubmissionDate = new DateTime(2026, 4, 5), CompletionDate = new DateTime(2026, 4, 10),
                Description = "Вопрос по размеру выходного пособия при увольнении",
                Result = "Проведена консультация, разъяснены нормы ст. 178 ТК РФ"
            },
            new Appeal
            {
                CitizenId = 4, RequestTypeId = 4, StatusId = StatusIds.InProgress, EmployeeId = 2,
                SubmissionDate = new DateTime(2026, 4, 8), CompletionDate = null,
                Description = "Хочу пройти курсы переподготовки на программиста", Result = null
            },
            new Appeal
            {
                CitizenId = 5, RequestTypeId = 5, StatusId = StatusIds.Completed, EmployeeId = 1,
                SubmissionDate = new DateTime(2026, 4, 10), CompletionDate = new DateTime(2026, 4, 15),
                Description = "Задержка выплаты пособия по безработице за март",
                Result = "Выплата произведена, задержка устранена"
            });
        await context.SaveChangesAsync();
    }
}

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using ObrashcheniyaWeb.Models;

namespace ObrashcheniyaWeb.Models.ViewModels;

/// <summary>Форма авторизации (Рисунок 9 диплома).</summary>
public class LoginViewModel
{
    [Required(ErrorMessage = "Введите логин.")]
    [Display(Name = "Логин")]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите пароль.")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

/// <summary>
/// Фильтр и результаты списка обращений (форма пользователя/админа).
/// Объединяет фильтрацию по статусу/типу/датам и поиск по ФИО (из диплома).
/// </summary>
public class AppealFilterViewModel
{
    [Display(Name = "Поиск по ФИО гражданина")]
    public string? Search { get; set; }

    [Display(Name = "Статус")]
    public int? StatusId { get; set; }

    [Display(Name = "Тип обращения")]
    public int? RequestTypeId { get; set; }

    [Display(Name = "Дата с")]
    [DataType(DataType.Date)]
    public DateTime? DateFrom { get; set; }

    [Display(Name = "Дата по")]
    [DataType(DataType.Date)]
    public DateTime? DateTo { get; set; }

    public List<Appeal> Appeals { get; set; } = new();
    public SelectList? Statuses { get; set; }
    public SelectList? RequestTypes { get; set; }
}

/// <summary>Данные для формы создания обращения (Рисунок 11 диплома).</summary>
public class CreateAppealViewModel
{
    [Required(ErrorMessage = "Выберите гражданина.")]
    [Display(Name = "Гражданин")]
    public int CitizenId { get; set; }

    [Required(ErrorMessage = "Выберите тип обращения.")]
    [Display(Name = "Тип обращения")]
    public int RequestTypeId { get; set; }

    [Required(ErrorMessage = "Введите описание сути вопроса.")]
    [Display(Name = "Описание")]
    public string Description { get; set; } = string.Empty;

    public SelectList? Citizens { get; set; }
    public SelectList? RequestTypes { get; set; }
}

/// <summary>Изменение статуса обращения (кнопка «Изменить статус» у админа).</summary>
public class ChangeStatusViewModel
{
    public int AppealId { get; set; }
    public string CitizenName { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;

    [Required(ErrorMessage = "Выберите статус.")]
    [Display(Name = "Новый статус")]
    public int StatusId { get; set; }

    [Display(Name = "Дата исполнения")]
    [DataType(DataType.Date)]
    public DateTime? CompletionDate { get; set; }

    [Display(Name = "Результат рассмотрения")]
    public string? Result { get; set; }

    public SelectList? Statuses { get; set; }
}

/// <summary>Обработка обращения: назначение ответственного + результат (кнопка «Обработать»).</summary>
public class ProcessAppealViewModel
{
    public int AppealId { get; set; }
    public string CitizenName { get; set; } = string.Empty;
    public string RequestTypeName { get; set; } = string.Empty;
    public string? Description { get; set; }

    [Required(ErrorMessage = "Назначьте ответственного сотрудника.")]
    [Display(Name = "Ответственный сотрудник")]
    public int EmployeeId { get; set; }

    [Required(ErrorMessage = "Выберите статус.")]
    [Display(Name = "Статус")]
    public int StatusId { get; set; }

    [Display(Name = "Дата исполнения")]
    [DataType(DataType.Date)]
    public DateTime? CompletionDate { get; set; }

    [Display(Name = "Результат рассмотрения")]
    public string? Result { get; set; }

    public SelectList? Employees { get; set; }
    public SelectList? Statuses { get; set; }
}

/// <summary>Статистика для Dashboard (главная страница).</summary>
public class DashboardViewModel
{
    public int TotalAppeals { get; set; }
    public int TotalCitizens { get; set; }
    public int TotalEmployees { get; set; }

    public int RegisteredCount { get; set; }
    public int InProgressCount { get; set; }
    public int CompletedCount { get; set; }
    public int RejectedCount { get; set; }

    public List<(string Name, int Count)> ByType { get; set; } = new();
    public List<(string Name, int Count)> ByEmployee { get; set; } = new();
    public List<Appeal> RecentAppeals { get; set; } = new();

    public int ActiveCount => RegisteredCount + InProgressCount;
    public double CompletionRate =>
        TotalAppeals == 0 ? 0 : Math.Round((double)CompletedCount / TotalAppeals * 100, 1);
}

/// <summary>
/// Форма сотрудника. Пароль отделён от сущности: при редактировании
/// пустое поле означает «не менять пароль» (раздел администрирования диплома).
/// </summary>
public class EmployeeFormModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Укажите ФИО сотрудника.")]
    [StringLength(100)]
    [Display(Name = "ФИО")]
    public string FullName { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Должность")]
    public string? Position { get; set; }

    [Required(ErrorMessage = "Укажите логин.")]
    [StringLength(50)]
    [Display(Name = "Логин")]
    public string Login { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "Выберите роль.")]
    [Display(Name = "Роль")]
    public string Role { get; set; } = Roles.User;
}

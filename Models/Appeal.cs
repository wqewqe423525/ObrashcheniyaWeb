using System.ComponentModel.DataAnnotations;

namespace ObrashcheniyaWeb.Models;

/// <summary>
/// Обращение — центральная сущность системы. Таблица БД — «Обращения».
/// Связана со всеми справочниками через четыре внешних ключа (из диплома).
/// </summary>
public class Appeal
{
    /// <summary>ID_Обращения — первичный ключ (IDENTITY).</summary>
    public int Id { get; set; }

    /// <summary>ID_Гражданина — FK → Граждане, NOT NULL.</summary>
    [Required(ErrorMessage = "Выберите гражданина.")]
    [Display(Name = "Гражданин")]
    public int CitizenId { get; set; }
    public Citizen? Citizen { get; set; }

    /// <summary>ID_Типа — FK → Типы_обращений, NOT NULL.</summary>
    [Required(ErrorMessage = "Выберите тип обращения.")]
    [Display(Name = "Тип обращения")]
    public int RequestTypeId { get; set; }
    public RequestType? RequestType { get; set; }

    /// <summary>ID_Статуса — FK → Статусы, NOT NULL.</summary>
    [Required]
    [Display(Name = "Статус")]
    public int StatusId { get; set; }
    public Status? Status { get; set; }

    /// <summary>ID_Сотрудника — FK → Сотрудники (ответственный), NOT NULL.</summary>
    [Required]
    [Display(Name = "Ответственный")]
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    /// <summary>Дата_поступления — DATE, NOT NULL, DEFAULT GETDATE().</summary>
    [DataType(DataType.Date)]
    [Display(Name = "Дата поступления")]
    public DateTime SubmissionDate { get; set; }

    /// <summary>Дата_исполнения — DATE, допускает NULL.</summary>
    [DataType(DataType.Date)]
    [Display(Name = "Дата исполнения")]
    public DateTime? CompletionDate { get; set; }

    /// <summary>Описание — NVARCHAR(MAX). Суть вопроса заявителя.</summary>
    [Required(ErrorMessage = "Введите описание обращения.")]
    [Display(Name = "Описание")]
    public string? Description { get; set; }

    /// <summary>Результат — NVARCHAR(MAX). Итог рассмотрения.</summary>
    [Display(Name = "Результат рассмотрения")]
    public string? Result { get; set; }
}

/// <summary>
/// Идентификаторы статусов соответствуют порядку INSERT в Приложении А диплома.
/// Используются в бизнес-логике (например, новый обращение = «Зарегистрировано»).
/// </summary>
public static class StatusIds
{
    public const int Registered = 1; // Зарегистрировано
    public const int InProgress = 2; // В работе
    public const int Completed  = 3; // Исполнено
    public const int Rejected   = 4; // Отказано
}

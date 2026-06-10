using System.ComponentModel.DataAnnotations;

namespace ObrashcheniyaWeb.Models;

/// <summary>
/// Сотрудник — пользователь системы с учётными данными. Таблица БД — «Сотрудники».
/// Поле «Роль» определяет набор доступных функций (из диплома).
/// </summary>
public class Employee
{
    /// <summary>ID_Сотрудника — первичный ключ (IDENTITY).</summary>
    public int Id { get; set; }

    /// <summary>ФИО — NVARCHAR(100), NOT NULL.</summary>
    [Required(ErrorMessage = "Укажите ФИО сотрудника.")]
    [StringLength(100, ErrorMessage = "ФИО не должно превышать 100 символов.")]
    [Display(Name = "ФИО")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>Должность — NVARCHAR(100).</summary>
    [StringLength(100, ErrorMessage = "Должность не должна превышать 100 символов.")]
    [Display(Name = "Должность")]
    public string? Position { get; set; }

    /// <summary>Логин — NVARCHAR(50), NOT NULL.</summary>
    [Required(ErrorMessage = "Укажите логин.")]
    [StringLength(50, ErrorMessage = "Логин не должен превышать 50 символов.")]
    [Display(Name = "Логин")]
    public string Login { get; set; } = string.Empty;

    /// <summary>
    /// Пароль — NVARCHAR(... ), NOT NULL.
    /// В дипломе хранился в открытом виде; здесь хранится PBKDF2-хэш
    /// (рекомендация из раздела 3.3 «Хранение паролей»).
    /// </summary>
    [Required]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    /// <summary>Роль — NVARCHAR(20), DEFAULT «пользователь».</summary>
    [Required(ErrorMessage = "Укажите роль.")]
    [StringLength(20)]
    [Display(Name = "Роль")]
    public string Role { get; set; } = Roles.User;

    /// <summary>Обратная навигация: обращения, за которые отвечает сотрудник.</summary>
    public ICollection<Appeal> Appeals { get; set; } = new List<Appeal>();
}

/// <summary>Константы ролей системы — единый источник значений (из диплома).</summary>
public static class Roles
{
    public const string Admin = "администратор";
    public const string User = "пользователь";

    public static readonly string[] All = { Admin, User };
}

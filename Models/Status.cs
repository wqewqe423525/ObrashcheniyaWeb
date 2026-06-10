using System.ComponentModel.DataAnnotations;

namespace ObrashcheniyaWeb.Models;

/// <summary>
/// Справочник состояний обращения. Таблица БД — «Статусы».
/// Из диплома: «Зарегистрировано», «В работе», «Исполнено», «Отказано».
/// </summary>
public class Status
{
    /// <summary>ID_Статуса — первичный ключ (IDENTITY).</summary>
    public int Id { get; set; }

    /// <summary>Название — NVARCHAR(50), NOT NULL.</summary>
    [Required(ErrorMessage = "Укажите название статуса.")]
    [StringLength(50, ErrorMessage = "Название не должно превышать 50 символов.")]
    [Display(Name = "Название")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Обратная навигация: обращения с данным статусом.</summary>
    public ICollection<Appeal> Appeals { get; set; } = new List<Appeal>();
}

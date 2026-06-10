using System.ComponentModel.DataAnnotations;

namespace ObrashcheniyaWeb.Models;

/// <summary>
/// Справочник категорий заявок. Таблица БД — «Типы_обращений».
/// Из диплома: Поиск работы, Регистрация безработного, Консультация,
/// Профессиональная переподготовка, Выплата пособия.
/// </summary>
public class RequestType
{
    /// <summary>ID_Типа — первичный ключ (IDENTITY).</summary>
    public int Id { get; set; }

    /// <summary>Название — NVARCHAR(100), NOT NULL.</summary>
    [Required(ErrorMessage = "Укажите название типа обращения.")]
    [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов.")]
    [Display(Name = "Название")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Описание — NVARCHAR(255), необязательное.</summary>
    [StringLength(255, ErrorMessage = "Описание не должно превышать 255 символов.")]
    [Display(Name = "Описание")]
    public string? Description { get; set; }

    /// <summary>Обратная навигация: обращения данного типа.</summary>
    public ICollection<Appeal> Appeals { get; set; } = new List<Appeal>();
}

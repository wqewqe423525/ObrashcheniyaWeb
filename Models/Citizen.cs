using System.ComponentModel.DataAnnotations;

namespace ObrashcheniyaWeb.Models;

/// <summary>
/// Гражданин-заявитель. Таблица БД — «Граждане».
/// ФИО разделено на три поля согласно российской практике (из диплома).
/// </summary>
public class Citizen
{
    /// <summary>ID_Гражданина — первичный ключ (IDENTITY).</summary>
    public int Id { get; set; }

    /// <summary>Фамилия — NVARCHAR(50), NOT NULL.</summary>
    [Required(ErrorMessage = "Укажите фамилию.")]
    [StringLength(50, ErrorMessage = "Фамилия не должна превышать 50 символов.")]
    [Display(Name = "Фамилия")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>Имя — NVARCHAR(50), NOT NULL.</summary>
    [Required(ErrorMessage = "Укажите имя.")]
    [StringLength(50, ErrorMessage = "Имя не должно превышать 50 символов.")]
    [Display(Name = "Имя")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Отчество — NVARCHAR(50), допускает NULL (не у всех есть отчество).</summary>
    [StringLength(50, ErrorMessage = "Отчество не должно превышать 50 символов.")]
    [Display(Name = "Отчество")]
    public string? MiddleName { get; set; }

    /// <summary>Дата_рождения — DATE.</summary>
    [DataType(DataType.Date)]
    [Display(Name = "Дата рождения")]
    public DateTime? BirthDate { get; set; }

    /// <summary>Телефон — NVARCHAR(20).</summary>
    [StringLength(20, ErrorMessage = "Телефон не должен превышать 20 символов.")]
    [Phone(ErrorMessage = "Некорректный номер телефона.")]
    [Display(Name = "Телефон")]
    public string? Phone { get; set; }

    /// <summary>Email — NVARCHAR(100).</summary>
    [StringLength(100, ErrorMessage = "Email не должен превышать 100 символов.")]
    [EmailAddress(ErrorMessage = "Некорректный адрес электронной почты.")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    /// <summary>Адрес — NVARCHAR(255).</summary>
    [StringLength(255, ErrorMessage = "Адрес не должен превышать 255 символов.")]
    [Display(Name = "Адрес")]
    public string? Address { get; set; }

    /// <summary>Обратная навигация: обращения данного гражданина.</summary>
    public ICollection<Appeal> Appeals { get; set; } = new List<Appeal>();

    /// <summary>Удобное представление ФИО одной строкой.</summary>
    public string FullName =>
        string.Join(' ', new[] { LastName, FirstName, MiddleName }
            .Where(p => !string.IsNullOrWhiteSpace(p)));

    /// <summary>Фамилия и инициалы, например «Иванов И.П.».</summary>
    public string ShortName
    {
        get
        {
            var initials = string.Empty;
            if (!string.IsNullOrWhiteSpace(FirstName))
                initials += $" {char.ToUpper(FirstName[0])}.";
            if (!string.IsNullOrWhiteSpace(MiddleName))
                initials += $"{char.ToUpper(MiddleName[0])}.";
            return $"{LastName}{initials}".Trim();
        }
    }
}

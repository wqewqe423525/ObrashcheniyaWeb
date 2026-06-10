using Microsoft.EntityFrameworkCore;
using ObrashcheniyaWeb.Models;

namespace ObrashcheniyaWeb.Data;

/// <summary>
/// Контекст БД «УчетОбращений». Сохраняет структуру таблиц и полей 1-в-1
/// с дипломным проектом (Приложение А): кириллические имена таблиц/столбцов,
/// автоинкрементные первичные ключи, внешние ключи и ограничения NOT NULL.
/// EF Core заменяет прямые SQL-запросы ADO.NET, использованные в дипломе.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Status> Statuses => Set<Status>();
    public DbSet<RequestType> RequestTypes => Set<RequestType>();
    public DbSet<Citizen> Citizens => Set<Citizen>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Appeal> Appeals => Set<Appeal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---------- Статусы ----------
        modelBuilder.Entity<Status>(e =>
        {
            e.ToTable("Статусы");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID_Статуса").ValueGeneratedOnAdd();
            e.Property(x => x.Name).HasColumnName("Название")
                .HasMaxLength(50).IsRequired();
        });

        // ---------- Типы_обращений ----------
        modelBuilder.Entity<RequestType>(e =>
        {
            e.ToTable("Типы_обращений");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID_Типа").ValueGeneratedOnAdd();
            e.Property(x => x.Name).HasColumnName("Название")
                .HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasColumnName("Описание")
                .HasMaxLength(255);
        });

        // ---------- Граждане ----------
        modelBuilder.Entity<Citizen>(e =>
        {
            e.ToTable("Граждане");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID_Гражданина").UseIdentityColumn();
            e.Property(x => x.LastName).HasColumnName("Фамилия")
                .HasMaxLength(50).IsRequired();
            e.Property(x => x.FirstName).HasColumnName("Имя")
                .HasMaxLength(50).IsRequired();
            e.Property(x => x.MiddleName).HasColumnName("Отчество")
                .HasMaxLength(50);
            e.Property(x => x.BirthDate).HasColumnName("Дата_рождения")
                .HasColumnType("date");
            e.Property(x => x.Phone).HasColumnName("Телефон").HasMaxLength(20);
            e.Property(x => x.Email).HasColumnName("Email").HasMaxLength(100);
            e.Property(x => x.Address).HasColumnName("Адрес").HasMaxLength(255);
            e.Ignore(x => x.FullName);
            e.Ignore(x => x.ShortName);
        });

        // ---------- Сотрудники ----------
        modelBuilder.Entity<Employee>(e =>
        {
            e.ToTable("Сотрудники");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID_Сотрудника").UseIdentityColumn();
            e.Property(x => x.FullName).HasColumnName("ФИО")
                .HasMaxLength(100).IsRequired();
            e.Property(x => x.Position).HasColumnName("Должность").HasMaxLength(100);
            e.Property(x => x.Login).HasColumnName("Логин")
                .HasMaxLength(50).IsRequired();
            // Расширено до 200 символов для хранения PBKDF2-хэша (вместо открытого пароля).
            e.Property(x => x.Password).HasColumnName("Пароль")
                .HasMaxLength(200).IsRequired();
            e.Property(x => x.Role).HasColumnName("Роль")
                .HasMaxLength(20).IsRequired().HasDefaultValue(Roles.User);
            e.HasIndex(x => x.Login).IsUnique();
        });

        // ---------- Обращения ----------
        modelBuilder.Entity<Appeal>(e =>
        {
            e.ToTable("Обращения");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID_Обращения").UseIdentityColumn();
            e.Property(x => x.CitizenId).HasColumnName("ID_Гражданина").IsRequired();
            e.Property(x => x.RequestTypeId).HasColumnName("ID_Типа").IsRequired();
            e.Property(x => x.StatusId).HasColumnName("ID_Статуса").IsRequired();
            e.Property(x => x.EmployeeId).HasColumnName("ID_Сотрудника").IsRequired();
            e.Property(x => x.SubmissionDate).HasColumnName("Дата_поступления")
                .HasColumnType("date").IsRequired()
                .HasDefaultValueSql("CURRENT_DATE");
            e.Property(x => x.CompletionDate).HasColumnName("Дата_исполнения")
                .HasColumnType("date");
            e.Property(x => x.Description).HasColumnName("Описание")
                .HasColumnType("nvarchar(max)");
            e.Property(x => x.Result).HasColumnName("Результат")
                .HasColumnType("nvarchar(max)");

            // Связи 1:N. Restrict — запрет удаления справочной записи при наличии
            // связанных обращений (ссылочная целостность, как в дипломе).
            e.HasOne(x => x.Citizen).WithMany(c => c.Appeals)
                .HasForeignKey(x => x.CitizenId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.RequestType).WithMany(t => t.Appeals)
                .HasForeignKey(x => x.RequestTypeId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Status).WithMany(s => s.Appeals)
                .HasForeignKey(x => x.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Employee).WithMany(emp => emp.Appeals)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

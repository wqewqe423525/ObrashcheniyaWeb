using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ObrashcheniyaWeb.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Граждане",
                columns: table => new
                {
                    ID_Гражданина = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Фамилия = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Имя = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Отчество = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Дата_рождения = table.Column<DateTime>(type: "date", nullable: true),
                    Телефон = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Адрес = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Граждане", x => x.ID_Гражданина);
                });

            migrationBuilder.CreateTable(
                name: "Сотрудники",
                columns: table => new
                {
                    ID_Сотрудника = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ФИО = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Должность = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Логин = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Пароль = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Роль = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "пользователь")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Сотрудники", x => x.ID_Сотрудника);
                });

            migrationBuilder.CreateTable(
                name: "Статусы",
                columns: table => new
                {
                    ID_Статуса = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Название = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Статусы", x => x.ID_Статуса);
                });

            migrationBuilder.CreateTable(
                name: "Типы_обращений",
                columns: table => new
                {
                    ID_Типа = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Название = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Описание = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Типы_обращений", x => x.ID_Типа);
                });

            migrationBuilder.CreateTable(
                name: "Обращения",
                columns: table => new
                {
                    ID_Обращения = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ID_Гражданина = table.Column<int>(type: "integer", nullable: false),
                    ID_Типа = table.Column<int>(type: "integer", nullable: false),
                    ID_Статуса = table.Column<int>(type: "integer", nullable: false),
                    ID_Сотрудника = table.Column<int>(type: "integer", nullable: false),
                    Дата_поступления = table.Column<DateTime>(type: "date", nullable: false, defaultValueSql: "CURRENT_DATE"),
                    Дата_исполнения = table.Column<DateTime>(type: "date", nullable: true),
                    Описание = table.Column<string>(type: "text", nullable: false),
                    Результат = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Обращения", x => x.ID_Обращения);
                    table.ForeignKey(
                        name: "FK_Обращения_Граждане_ID_Гражданина",
                        column: x => x.ID_Гражданина,
                        principalTable: "Граждане",
                        principalColumn: "ID_Гражданина",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Обращения_Сотрудники_ID_Сотрудника",
                        column: x => x.ID_Сотрудника,
                        principalTable: "Сотрудники",
                        principalColumn: "ID_Сотрудника",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Обращения_Статусы_ID_Статуса",
                        column: x => x.ID_Статуса,
                        principalTable: "Статусы",
                        principalColumn: "ID_Статуса",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Обращения_Типы_обращений_ID_Типа",
                        column: x => x.ID_Типа,
                        principalTable: "Типы_обращений",
                        principalColumn: "ID_Типа",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Обращения_ID_Гражданина",
                table: "Обращения",
                column: "ID_Гражданина");

            migrationBuilder.CreateIndex(
                name: "IX_Обращения_ID_Сотрудника",
                table: "Обращения",
                column: "ID_Сотрудника");

            migrationBuilder.CreateIndex(
                name: "IX_Обращения_ID_Статуса",
                table: "Обращения",
                column: "ID_Статуса");

            migrationBuilder.CreateIndex(
                name: "IX_Обращения_ID_Типа",
                table: "Обращения",
                column: "ID_Типа");

            migrationBuilder.CreateIndex(
                name: "IX_Сотрудники_Логин",
                table: "Сотрудники",
                column: "Логин",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Обращения");

            migrationBuilder.DropTable(
                name: "Граждане");

            migrationBuilder.DropTable(
                name: "Сотрудники");

            migrationBuilder.DropTable(
                name: "Статусы");

            migrationBuilder.DropTable(
                name: "Типы_обращений");
        }
    }
}

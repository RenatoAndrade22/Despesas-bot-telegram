using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Despesas.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionandoTelegramUpdateId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TelegramUpdateId",
                table: "ControleMensagens",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TelegramUpdateId",
                table: "ControleMensagens");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Despesas.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CriandoDespesasFornecedoresAgendamentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Fornecedor",
                table: "Despesas");

            migrationBuilder.AddColumn<int>(
                name: "FornecedorId",
                table: "Despesas",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Fornecedores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fornecedores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Agendamentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    DiaVencimento = table.Column<int>(type: "integer", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    FornecedorId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agendamentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Agendamentos_Fornecedores_FornecedorId",
                        column: x => x.FornecedorId,
                        principalTable: "Fornecedores",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Agendamentos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Despesas_FornecedorId",
                table: "Despesas",
                column: "FornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Agendamentos_FornecedorId",
                table: "Agendamentos",
                column: "FornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Agendamentos_UsuarioId",
                table: "Agendamentos",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Despesas_Fornecedores_FornecedorId",
                table: "Despesas",
                column: "FornecedorId",
                principalTable: "Fornecedores",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Despesas_Fornecedores_FornecedorId",
                table: "Despesas");

            migrationBuilder.DropTable(
                name: "Agendamentos");

            migrationBuilder.DropTable(
                name: "Fornecedores");

            migrationBuilder.DropIndex(
                name: "IX_Despesas_FornecedorId",
                table: "Despesas");

            migrationBuilder.DropColumn(
                name: "FornecedorId",
                table: "Despesas");

            migrationBuilder.AddColumn<string>(
                name: "Fornecedor",
                table: "Despesas",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}

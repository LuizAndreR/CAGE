using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CakeGestao.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlteracaoTabelaItensEstoqueUnidadeMedida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "UnidadeMedida",
                table: "ItensEstoque",
                type: "integer",
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UnidadeMedida",
                table: "ItensEstoque",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldMaxLength: 5);
        }
    }
}

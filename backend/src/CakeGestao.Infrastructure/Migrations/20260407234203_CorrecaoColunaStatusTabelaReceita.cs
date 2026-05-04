using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CakeGestao.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CorrecaoColunaStatusTabelaReceita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Ativo",
                table: "Receitas",
                newName: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Receitas",
                newName: "Ativo");
        }
    }
}

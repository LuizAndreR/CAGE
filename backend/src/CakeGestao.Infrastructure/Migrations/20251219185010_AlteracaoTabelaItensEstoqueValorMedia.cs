using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CakeGestao.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlteracaoTabelaItensEstoqueValorMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ValorMedia",
                table: "ItensEstoque",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValorMedia",
                table: "ItensEstoque");
        }
    }
}

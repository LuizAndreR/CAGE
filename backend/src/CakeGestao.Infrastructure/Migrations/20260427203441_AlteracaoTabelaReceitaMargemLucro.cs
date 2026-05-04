using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CakeGestao.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlteracaoTabelaReceitaMargemLucro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CustoTotalEstimado",
                table: "Receitas",
                newName: "CustoTotal");

            migrationBuilder.AddColumn<decimal>(
                name: "PercentualCustoExtra",
                table: "Receitas",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PercentualMargemLucro",
                table: "Receitas",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PercentualCustoExtra",
                table: "Receitas");

            migrationBuilder.DropColumn(
                name: "PercentualMargemLucro",
                table: "Receitas");

            migrationBuilder.RenameColumn(
                name: "CustoTotal",
                table: "Receitas",
                newName: "CustoTotalEstimado");
        }
    }
}

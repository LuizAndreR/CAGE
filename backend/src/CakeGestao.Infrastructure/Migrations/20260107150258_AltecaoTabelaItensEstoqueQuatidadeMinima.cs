using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CakeGestao.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AltecaoTabelaItensEstoqueQuatidadeMinima : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "PedidoId",
                table: "TransacoesFinanceiras",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<decimal>(
                name: "QuantidadeMinina",
                table: "ItensEstoque",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 3m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuantidadeMinina",
                table: "ItensEstoque");

            migrationBuilder.AlterColumn<int>(
                name: "PedidoId",
                table: "TransacoesFinanceiras",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}

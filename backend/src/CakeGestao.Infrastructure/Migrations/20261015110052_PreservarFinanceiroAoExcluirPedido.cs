using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CakeGestao.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PreservarFinanceiroAoExcluirPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransacoesFinanceiras_Pedidos_PedidoId",
                table: "TransacoesFinanceiras");

            migrationBuilder.AddForeignKey(
                name: "FK_TransacoesFinanceiras_Pedidos_PedidoId",
                table: "TransacoesFinanceiras",
                column: "PedidoId",
                principalTable: "Pedidos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransacoesFinanceiras_Pedidos_PedidoId",
                table: "TransacoesFinanceiras");

            migrationBuilder.AddForeignKey(
                name: "FK_TransacoesFinanceiras_Pedidos_PedidoId",
                table: "TransacoesFinanceiras",
                column: "PedidoId",
                principalTable: "Pedidos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

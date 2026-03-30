using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CakeGestao.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlteracaoTabelaEstoqueConvesao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PesoReferenciaEmGramas",
                table: "ItensEstoque",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnidadeReferenciaVolume",
                table: "ItensEstoque",
                type: "integer",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PesoReferenciaEmGramas",
                table: "ItensEstoque");

            migrationBuilder.DropColumn(
                name: "UnidadeReferenciaVolume",
                table: "ItensEstoque");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CakeGestao.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlternativasIngredientesEConsumoPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Quantidade",
                table: "Ingredientes",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "QuantidadeAtual",
                table: "Estoque",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldDefaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "ConsumosEstoquePedido",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PedidoId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    Quantidade = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    UnidadeMedida = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumosEstoquePedido", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsumosEstoquePedido_Estoque_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Estoque",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsumosEstoquePedido_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IngredienteAlternativas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IngredienteId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    Ordem = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngredienteAlternativas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IngredienteAlternativas_Estoque_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Estoque",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IngredienteAlternativas_Ingredientes_IngredienteId",
                        column: x => x.IngredienteId,
                        principalTable: "Ingredientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsumosEstoquePedido_ItemId",
                table: "ConsumosEstoquePedido",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumosEstoquePedido_PedidoId",
                table: "ConsumosEstoquePedido",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_IngredienteAlternativas_IngredienteId_ItemId",
                table: "IngredienteAlternativas",
                columns: new[] { "IngredienteId", "ItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IngredienteAlternativas_ItemId",
                table: "IngredienteAlternativas",
                column: "ItemId");
            // Pedidos já em produção não possuem histórico de baixa. Registre a base que
            // a versão anterior usaria na devolução, antes de permitir editar alternativas.
            migrationBuilder.Sql("""
                INSERT INTO "ConsumosEstoquePedido" ("PedidoId", "ItemId", "Quantidade", "UnidadeMedida")
                SELECT p."Id", i."ItemId", SUM(i."Quantidade" * ip."Quantidade"), i."UnidadeMedida"
                FROM "Pedidos" p
                JOIN "ItensPedido" ip ON ip."PedidoId" = p."Id"
                JOIN "Ingredientes" i ON i."ReceitaId" = ip."ReceitaId"
                WHERE p."StatusPedido" = 1
                GROUP BY p."Id", i."ItemId", i."UnidadeMedida";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsumosEstoquePedido");

            migrationBuilder.DropTable(
                name: "IngredienteAlternativas");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantidade",
                table: "Ingredientes",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,6)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.AlterColumn<decimal>(
                name: "QuantidadeAtual",
                table: "Estoque",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,6)",
                oldPrecision: 18,
                oldScale: 6,
                oldDefaultValue: 0m);
        }
    }
}

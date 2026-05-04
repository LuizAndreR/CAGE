using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CakeGestao.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlteracaoTabelaPedidoEItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ingredientes_ItensEstoque_ItemId",
                table: "Ingredientes");

            migrationBuilder.DropTable(
                name: "ItensEstoque");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Pedidos");

            migrationBuilder.RenameColumn(
                name: "DataPedido",
                table: "Pedidos",
                newName: "DataCriacao");

            migrationBuilder.AlterColumn<decimal>(
                name: "ValorTotal",
                table: "Pedidos",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "Pedidos",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataEntrega",
                table: "Pedidos",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataPagamento",
                table: "Pedidos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Pago",
                table: "Pedidos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "StatusPedido",
                table: "Pedidos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TelefoneCliente",
                table: "Pedidos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ValorUnitario",
                table: "ItensPedido",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.CreateTable(
                name: "Estoque",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    QuantidadeAtual = table.Column<decimal>(type: "numeric(10,2)", nullable: false, defaultValue: 0m),
                    UnidadeMedida = table.Column<int>(type: "integer", nullable: false),
                    QuantidadeMinina = table.Column<decimal>(type: "numeric(10,2)", nullable: false, defaultValue: 3m),
                    ValorMedia = table.Column<decimal>(type: "numeric(10,2)", nullable: false, defaultValue: 0m),
                    UnidadeReferenciaVolume = table.Column<int>(type: "integer", maxLength: 20, nullable: true),
                    PesoReferenciaEmGramas = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estoque", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Estoque_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Estoque_EmpresaId",
                table: "Estoque",
                column: "EmpresaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ingredientes_Estoque_ItemId",
                table: "Ingredientes",
                column: "ItemId",
                principalTable: "Estoque",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ingredientes_Estoque_ItemId",
                table: "Ingredientes");

            migrationBuilder.DropTable(
                name: "Estoque");

            migrationBuilder.DropColumn(
                name: "DataPagamento",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "Pago",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "StatusPedido",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "TelefoneCliente",
                table: "Pedidos");

            migrationBuilder.RenameColumn(
                name: "DataCriacao",
                table: "Pedidos",
                newName: "DataPedido");

            migrationBuilder.AlterColumn<decimal>(
                name: "ValorTotal",
                table: "Pedidos",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "Pedidos",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataEntrega",
                table: "Pedidos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Pedidos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<decimal>(
                name: "ValorUnitario",
                table: "ItensPedido",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.CreateTable(
                name: "ItensEstoque",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PesoReferenciaEmGramas = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    QuantidadeAtual = table.Column<decimal>(type: "numeric(10,2)", nullable: false, defaultValue: 0m),
                    QuantidadeMinina = table.Column<decimal>(type: "numeric(10,2)", nullable: false, defaultValue: 3m),
                    UnidadeMedida = table.Column<int>(type: "integer", nullable: false),
                    UnidadeReferenciaVolume = table.Column<int>(type: "integer", maxLength: 20, nullable: true),
                    ValorMedia = table.Column<decimal>(type: "numeric(10,2)", nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensEstoque", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItensEstoque_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItensEstoque_EmpresaId",
                table: "ItensEstoque",
                column: "EmpresaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ingredientes_ItensEstoque_ItemId",
                table: "Ingredientes",
                column: "ItemId",
                principalTable: "ItensEstoque",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

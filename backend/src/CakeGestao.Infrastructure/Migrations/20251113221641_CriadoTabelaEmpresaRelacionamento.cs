using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CakeGestao.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CriadoTabelaEmpresaRelacionamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "Usuarios",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "TransacoesFinanceiras",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "Receitas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "Pedidos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "ItensEstoque",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Empresa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Status = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresa", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_EmpresaId",
                table: "Usuarios",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_TransacoesFinanceiras_EmpresaId",
                table: "TransacoesFinanceiras",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Receitas_EmpresaId",
                table: "Receitas",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_EmpresaId",
                table: "Pedidos",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ItensEstoque_EmpresaId",
                table: "ItensEstoque",
                column: "EmpresaId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItensEstoque_Empresa_EmpresaId",
                table: "ItensEstoque",
                column: "EmpresaId",
                principalTable: "Empresa",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Empresa_EmpresaId",
                table: "Pedidos",
                column: "EmpresaId",
                principalTable: "Empresa",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Receitas_Empresa_EmpresaId",
                table: "Receitas",
                column: "EmpresaId",
                principalTable: "Empresa",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TransacoesFinanceiras_Empresa_EmpresaId",
                table: "TransacoesFinanceiras",
                column: "EmpresaId",
                principalTable: "Empresa",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Empresa_EmpresaId",
                table: "Usuarios",
                column: "EmpresaId",
                principalTable: "Empresa",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItensEstoque_Empresa_EmpresaId",
                table: "ItensEstoque");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Empresa_EmpresaId",
                table: "Pedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_Receitas_Empresa_EmpresaId",
                table: "Receitas");

            migrationBuilder.DropForeignKey(
                name: "FK_TransacoesFinanceiras_Empresa_EmpresaId",
                table: "TransacoesFinanceiras");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Empresa_EmpresaId",
                table: "Usuarios");

            migrationBuilder.DropTable(
                name: "Empresa");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_EmpresaId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_TransacoesFinanceiras_EmpresaId",
                table: "TransacoesFinanceiras");

            migrationBuilder.DropIndex(
                name: "IX_Receitas_EmpresaId",
                table: "Receitas");

            migrationBuilder.DropIndex(
                name: "IX_Pedidos_EmpresaId",
                table: "Pedidos");

            migrationBuilder.DropIndex(
                name: "IX_ItensEstoque_EmpresaId",
                table: "ItensEstoque");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "TransacoesFinanceiras");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Receitas");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "ItensEstoque");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CakeGestao.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlteracaoFinanceiraCancelamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CanceladoPorUsuarioId",
                table: "TransacoesFinanceiras",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCancelamento",
                table: "TransacoesFinanceiras",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCancelado",
                table: "TransacoesFinanceiras",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MotivoCancelamento",
                table: "TransacoesFinanceiras",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanceladoPorUsuarioId",
                table: "TransacoesFinanceiras");

            migrationBuilder.DropColumn(
                name: "DataCancelamento",
                table: "TransacoesFinanceiras");

            migrationBuilder.DropColumn(
                name: "IsCancelado",
                table: "TransacoesFinanceiras");

            migrationBuilder.DropColumn(
                name: "MotivoCancelamento",
                table: "TransacoesFinanceiras");
        }
    }
}

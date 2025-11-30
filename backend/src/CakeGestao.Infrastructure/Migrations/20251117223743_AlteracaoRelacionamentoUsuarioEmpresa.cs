using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CakeGestao.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlteracaoRelacionamentoUsuarioEmpresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Removemos a FK (mantém como estava)
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Empresa_EmpresaId",
                table: "Usuarios");

            // 2. Alteramos o Usuario.EmpresaId (mantém como estava)
            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "Usuarios",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            // 3. CORREÇÃO AQUI:
            // Em vez de usar AlterColumn, usamos SQL direto para converter.
            // O 'USING 0' força todos os valores antigos a virarem 0, evitando erro de conversão.
            migrationBuilder.Sql("ALTER TABLE \"Empresa\" ALTER COLUMN \"Status\" TYPE integer USING 0;");

            // Opcional: Se precisar restaurar a restrição de Not Null ou Default após converter
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Empresa",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            // 4. Adicionamos a FK de volta (mantém como estava)
            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Empresa_EmpresaId",
                table: "Usuarios",
                column: "EmpresaId",
                principalTable: "Empresa",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Empresa_EmpresaId",
                table: "Usuarios");

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "Usuarios",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Empresa",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldMaxLength: 150);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Empresa_EmpresaId",
                table: "Usuarios",
                column: "EmpresaId",
                principalTable: "Empresa",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

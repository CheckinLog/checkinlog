using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CheckinLog.Migrations
{
    /// <inheritdoc />
    public partial class AtualizarTabelaCheckin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Placa",
                table: "Checkins",
                newName: "Nome");

            migrationBuilder.RenameColumn(
                name: "Motorista",
                table: "Checkins",
                newName: "Matricula");

            migrationBuilder.RenameColumn(
                name: "DataEntrada",
                table: "Checkins",
                newName: "DataCheckin");

            migrationBuilder.AddColumn<string>(
                name: "CaminhoCNH",
                table: "Checkins",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CaminhoCursoDefensiva",
                table: "Checkins",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Checkins",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Empresa",
                table: "Checkins",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Funcao",
                table: "Checkins",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CaminhoCNH",
                table: "Checkins");

            migrationBuilder.DropColumn(
                name: "CaminhoCursoDefensiva",
                table: "Checkins");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Checkins");

            migrationBuilder.DropColumn(
                name: "Empresa",
                table: "Checkins");

            migrationBuilder.DropColumn(
                name: "Funcao",
                table: "Checkins");

            migrationBuilder.RenameColumn(
                name: "Nome",
                table: "Checkins",
                newName: "Placa");

            migrationBuilder.RenameColumn(
                name: "Matricula",
                table: "Checkins",
                newName: "Motorista");

            migrationBuilder.RenameColumn(
                name: "DataCheckin",
                table: "Checkins",
                newName: "DataEntrada");
        }
    }
}

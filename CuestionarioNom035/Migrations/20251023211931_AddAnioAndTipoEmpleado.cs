using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuestionarioNom035.Migrations
{
    /// <inheritdoc />
    public partial class AddAnioAndTipoEmpleado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TipoEmpleado",
                table: "Participantes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Anio",
                table: "Cuestionarios",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipoEmpleado",
                table: "Participantes");

            migrationBuilder.DropColumn(
                name: "Anio",
                table: "Cuestionarios");
        }
    }
}

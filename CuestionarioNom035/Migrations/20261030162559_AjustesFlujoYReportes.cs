using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuestionarioNom035.Migrations
{
    /// <inheritdoc />
    public partial class AjustesFlujoYReportes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Areas");

            migrationBuilder.DropIndex(
                name: "IX_Respuestas_CuestionarioId",
                table: "Respuestas");

            migrationBuilder.AddColumn<Guid>(
                name: "Token",
                table: "CuestionarioSesiones",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<string>(
                name: "AreaNombre",
                table: "Cuestionarios",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SubareaNombre",
                table: "Cuestionarios",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TipoEmpleado",
                table: "Cuestionarios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Copia al cuestionario el área/subárea/tipo que hoy tiene el participante
            migrationBuilder.Sql(@"
UPDATE c SET c.AreaNombre = p.AreaNombre, c.SubareaNombre = p.SubareaNombre, c.TipoEmpleado = p.TipoEmpleado
FROM Cuestionarios c
INNER JOIN Participantes p ON p.Id = c.ParticipanteId;");

            // Cuestionarios que llegaron al final pero se quedaron sin fecha de fin
            migrationBuilder.Sql(@"
UPDATE c SET c.Fin = c.Inicio
FROM Cuestionarios c
INNER JOIN CuestionarioSesiones s ON s.CuestionarioId = c.Id
WHERE s.IndiceActual > 72 AND c.Fin IS NULL;");

            // Sesiones terminadas que se quedaron en la pregunta 72
            migrationBuilder.Sql(@"
UPDATE s SET s.IndiceActual = 73
FROM CuestionarioSesiones s
INNER JOIN Cuestionarios c ON c.Id = s.CuestionarioId
WHERE c.Fin IS NOT NULL AND s.IndiceActual <= 72;");

            // Respuestas duplicadas (misma pregunta en el mismo cuestionario): se conserva la primera
            migrationBuilder.Sql(@"
DELETE r FROM Respuestas r
WHERE EXISTS (SELECT 1 FROM Respuestas r2
              WHERE r2.CuestionarioId = r.CuestionarioId
                AND r2.NumeroPregunta = r.NumeroPregunta
                AND r2.Id < r.Id);");

            migrationBuilder.CreateIndex(
                name: "IX_Respuestas_CuestionarioId_NumeroPregunta",
                table: "Respuestas",
                columns: new[] { "CuestionarioId", "NumeroPregunta" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CuestionarioSesiones_Token",
                table: "CuestionarioSesiones",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Respuestas_CuestionarioId_NumeroPregunta",
                table: "Respuestas");

            migrationBuilder.DropIndex(
                name: "IX_CuestionarioSesiones_Token",
                table: "CuestionarioSesiones");

            migrationBuilder.DropColumn(
                name: "Token",
                table: "CuestionarioSesiones");

            migrationBuilder.DropColumn(
                name: "AreaNombre",
                table: "Cuestionarios");

            migrationBuilder.DropColumn(
                name: "SubareaNombre",
                table: "Cuestionarios");

            migrationBuilder.DropColumn(
                name: "TipoEmpleado",
                table: "Cuestionarios");

            migrationBuilder.CreateTable(
                name: "Areas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Areas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Respuestas_CuestionarioId",
                table: "Respuestas",
                column: "CuestionarioId");
        }
    }
}

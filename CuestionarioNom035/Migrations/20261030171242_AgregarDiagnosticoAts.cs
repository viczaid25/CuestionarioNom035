using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuestionarioNom035.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDiagnosticoAts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AtsEvaluaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nomina = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PcLoginId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NombreCompleto = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Departamento = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Posicion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Edad = table.Column<int>(type: "int", nullable: false),
                    Antiguedad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Inicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Fin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Paso = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    AceptaDarDetalles = table.Column<bool>(type: "bit", nullable: true),
                    ComentarioDarDetalles = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DescripcionSuceso = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    AceptaComunicarRH = table.Column<bool>(type: "bit", nullable: true),
                    ComentarioComunicarRH = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TipoEvaluacion = table.Column<int>(type: "int", nullable: true),
                    SeguimientoExterno = table.Column<bool>(type: "bit", nullable: true),
                    SeguimientoInterno = table.Column<bool>(type: "bit", nullable: true),
                    DecisionAcudir = table.Column<bool>(type: "bit", nullable: true),
                    MotivoDecision = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RevisadoPor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaRevision = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtsEvaluaciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AtsRespuestas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EvaluacionId = table.Column<int>(type: "int", nullable: false),
                    Seccion = table.Column<int>(type: "int", nullable: false),
                    Numero = table.Column<int>(type: "int", nullable: false),
                    Si = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtsRespuestas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AtsRespuestas_AtsEvaluaciones_EvaluacionId",
                        column: x => x.EvaluacionId,
                        principalTable: "AtsEvaluaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AtsEvaluaciones_Estado",
                table: "AtsEvaluaciones",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_AtsEvaluaciones_PcLoginId",
                table: "AtsEvaluaciones",
                column: "PcLoginId");

            migrationBuilder.CreateIndex(
                name: "IX_AtsRespuestas_EvaluacionId_Seccion_Numero",
                table: "AtsRespuestas",
                columns: new[] { "EvaluacionId", "Seccion", "Numero" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AtsRespuestas");

            migrationBuilder.DropTable(
                name: "AtsEvaluaciones");
        }
    }
}

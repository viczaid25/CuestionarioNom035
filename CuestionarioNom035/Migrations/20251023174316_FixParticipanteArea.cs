using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuestionarioNom035.Migrations
{
    /// <inheritdoc />
    public partial class FixParticipanteArea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Participantes_Areas_AreaId",
                table: "Participantes");

            migrationBuilder.DropIndex(
                name: "IX_Participantes_AreaId",
                table: "Participantes");

            migrationBuilder.DropColumn(
                name: "AreaId",
                table: "Participantes");

            migrationBuilder.AddColumn<string>(
                name: "AreaNombre",
                table: "Participantes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AreaNombre",
                table: "Participantes");

            migrationBuilder.AddColumn<int>(
                name: "AreaId",
                table: "Participantes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Participantes_AreaId",
                table: "Participantes",
                column: "AreaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Participantes_Areas_AreaId",
                table: "Participantes",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

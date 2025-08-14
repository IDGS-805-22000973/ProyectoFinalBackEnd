using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoFinal.Migrations
{
    /// <inheritdoc />
    public partial class pruebaventas6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreadoPorAdminId",
                table: "Ventas",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_CreadoPorAdminId",
                table: "Ventas",
                column: "CreadoPorAdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_AspNetUsers_CreadoPorAdminId",
                table: "Ventas",
                column: "CreadoPorAdminId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_AspNetUsers_CreadoPorAdminId",
                table: "Ventas");

            migrationBuilder.DropIndex(
                name: "IX_Ventas_CreadoPorAdminId",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "CreadoPorAdminId",
                table: "Ventas");
        }
    }
}

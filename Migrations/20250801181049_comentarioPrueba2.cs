using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoFinal.Migrations
{
    /// <inheritdoc />
    public partial class comentarioPrueba2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Texto",
                table: "Comentarios",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "AdminRespuestaId",
                table: "Comentarios",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRespuesta",
                table: "Comentarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Respuesta",
                table: "Comentarios",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comentarios_AdminRespuestaId",
                table: "Comentarios",
                column: "AdminRespuestaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comentarios_AspNetUsers_AdminRespuestaId",
                table: "Comentarios",
                column: "AdminRespuestaId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comentarios_AspNetUsers_AdminRespuestaId",
                table: "Comentarios");

            migrationBuilder.DropIndex(
                name: "IX_Comentarios_AdminRespuestaId",
                table: "Comentarios");

            migrationBuilder.DropColumn(
                name: "AdminRespuestaId",
                table: "Comentarios");

            migrationBuilder.DropColumn(
                name: "FechaRespuesta",
                table: "Comentarios");

            migrationBuilder.DropColumn(
                name: "Respuesta",
                table: "Comentarios");

            migrationBuilder.AlterColumn<string>(
                name: "Texto",
                table: "Comentarios",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);
        }
    }
}

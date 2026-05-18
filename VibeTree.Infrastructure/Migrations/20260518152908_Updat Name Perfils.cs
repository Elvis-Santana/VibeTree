using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VibeTree.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatNamePerfils : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Perfil_Users_IdUser",
                table: "Perfil");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Perfil",
                table: "Perfil");

            migrationBuilder.RenameTable(
                name: "Perfil",
                newName: "perfils");

            migrationBuilder.RenameIndex(
                name: "IX_Perfil_IdUser",
                table: "perfils",
                newName: "IX_perfils_IdUser");

            migrationBuilder.AddPrimaryKey(
                name: "PK_perfils",
                table: "perfils",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_perfils_Users_IdUser",
                table: "perfils",
                column: "IdUser",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_perfils_Users_IdUser",
                table: "perfils");

            migrationBuilder.DropPrimaryKey(
                name: "PK_perfils",
                table: "perfils");

            migrationBuilder.RenameTable(
                name: "perfils",
                newName: "Perfil");

            migrationBuilder.RenameIndex(
                name: "IX_perfils_IdUser",
                table: "Perfil",
                newName: "IX_Perfil_IdUser");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Perfil",
                table: "Perfil",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Perfil_Users_IdUser",
                table: "Perfil",
                column: "IdUser",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

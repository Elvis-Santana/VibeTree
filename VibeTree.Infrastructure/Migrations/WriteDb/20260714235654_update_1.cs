using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VibeTree.Infrastructure.Migrations.WriteDb
{
    /// <inheritdoc />
    public partial class update_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PerfilId",
                table: "Users",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PerfilId",
                table: "Users",
                column: "PerfilId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_perfils_PerfilId",
                table: "Users",
                column: "PerfilId",
                principalTable: "perfils",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_perfils_PerfilId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_PerfilId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PerfilId",
                table: "Users");
        }
    }
}

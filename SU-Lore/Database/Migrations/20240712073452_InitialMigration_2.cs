using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SU_Lore.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EditingBy",
                table: "Pages");

            migrationBuilder.AddColumn<Guid>(
                name: "AccountId",
                table: "LogEntries",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LogEntries_AccountId",
                table: "LogEntries",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_LogEntries_Accounts_AccountId",
                table: "LogEntries",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LogEntries_Accounts_AccountId",
                table: "LogEntries");

            migrationBuilder.DropIndex(
                name: "IX_LogEntries_AccountId",
                table: "LogEntries");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "LogEntries");

            migrationBuilder.AddColumn<Guid>(
                name: "EditingBy",
                table: "Pages",
                type: "uuid",
                nullable: true);
        }
    }
}

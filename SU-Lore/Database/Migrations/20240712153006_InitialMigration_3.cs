using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SU_Lore.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration_3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PageGuid",
                table: "Pages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PageGuid",
                table: "Pages");
        }
    }
}

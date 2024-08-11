using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SU_Lore.Migrations
{
    /// <inheritdoc />
    public partial class PrivateFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPrivate",
                table: "Files",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPrivate",
                table: "Files");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SU_Lore.Migrations
{
    /// <inheritdoc />
    public partial class ForeignKeysPageFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PageFlag_Pages_PageId",
                table: "PageFlag");

            migrationBuilder.AlterColumn<int>(
                name: "PageId",
                table: "PageFlag",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PageFlag_Pages_PageId",
                table: "PageFlag",
                column: "PageId",
                principalTable: "Pages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PageFlag_Pages_PageId",
                table: "PageFlag");

            migrationBuilder.AlterColumn<int>(
                name: "PageId",
                table: "PageFlag",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_PageFlag_Pages_PageId",
                table: "PageFlag",
                column: "PageId",
                principalTable: "Pages",
                principalColumn: "Id");
        }
    }
}

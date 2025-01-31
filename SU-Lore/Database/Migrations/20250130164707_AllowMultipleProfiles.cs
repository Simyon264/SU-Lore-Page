using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SU_Lore.Migrations
{
    /// <inheritdoc />
    public partial class AllowMultipleProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Profiles_AccountId",
                table: "Profiles");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_AccountId",
                table: "Profiles",
                column: "AccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Profiles_AccountId",
                table: "Profiles");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_AccountId",
                table: "Profiles",
                column: "AccountId",
                unique: true);
        }
    }
}

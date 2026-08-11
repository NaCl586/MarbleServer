using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarbleServer.Migrations
{
    /// <inheritdoc />
    public partial class UniquePlayerLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Scores_PlayerId",
                table: "Scores");

            migrationBuilder.CreateIndex(
                name: "IX_Scores_PlayerId_Level",
                table: "Scores",
                columns: new[] { "PlayerId", "Level" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Scores_PlayerId_Level",
                table: "Scores");

            migrationBuilder.CreateIndex(
                name: "IX_Scores_PlayerId",
                table: "Scores",
                column: "PlayerId");
        }
    }
}

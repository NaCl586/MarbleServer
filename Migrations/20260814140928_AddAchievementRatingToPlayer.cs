using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarbleServer.Migrations
{
    /// <inheritdoc />
    public partial class AddAchievementRatingToPlayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AchievementRating",
                table: "Players",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AchievementRating",
                table: "Players");
        }
    }
}

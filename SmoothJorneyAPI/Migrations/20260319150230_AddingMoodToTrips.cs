using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmoothJorneyAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddingMoodToTrips : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AverageRating",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "Sentiment",
                table: "Reviews");

            migrationBuilder.AddColumn<string>(
                name: "Mood",
                table: "Trips",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Mood",
                table: "Trips");

            migrationBuilder.AddColumn<double>(
                name: "AverageRating",
                table: "Reviews",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Sentiment",
                table: "Reviews",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}

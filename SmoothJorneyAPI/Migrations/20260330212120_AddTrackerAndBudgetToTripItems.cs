using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmoothJorneyAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddTrackerAndBudgetToTripItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Cost",
                table: "TripItems",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Duration",
                table: "TripItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVisited",
                table: "TripItems",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cost",
                table: "TripItems");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "TripItems");

            migrationBuilder.DropColumn(
                name: "IsVisited",
                table: "TripItems");
        }
    }
}

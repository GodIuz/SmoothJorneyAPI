using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmoothJorneyAPI.Migrations
{
    /// <inheritdoc />
    public partial class CleanUpDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentCost",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "EstimatedCost",
                table: "TripItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CurrentCost",
                table: "Trips",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedCost",
                table: "TripItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}

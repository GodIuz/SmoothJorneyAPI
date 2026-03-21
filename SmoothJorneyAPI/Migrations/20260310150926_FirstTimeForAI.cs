using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmoothJorneyAPI.Migrations
{
    /// <inheritdoc />
    public partial class FirstTimeForAI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShareToken",
                table: "Trips");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShareToken",
                table: "Trips",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}

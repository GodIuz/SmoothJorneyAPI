using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmoothJorneyAPI.Migrations
{
    /// <inheritdoc />
    public partial class Finalizing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessPhoto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BusinessPhoto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessId = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessPhoto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessPhoto_Business_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Business",
                        principalColumn: "BusinessId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessPhoto_BusinessId",
                table: "BusinessPhoto",
                column: "BusinessId");
        }
    }
}

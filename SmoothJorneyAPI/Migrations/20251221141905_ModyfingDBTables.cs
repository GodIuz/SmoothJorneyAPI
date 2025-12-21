using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmoothJorneyAPI.Migrations
{
    /// <inheritdoc />
    public partial class ModyfingDBTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageContent",
                table: "BusinessImages",
                newName: "ImageData");

            migrationBuilder.AddColumn<bool>(
                name: "IsCover",
                table: "BusinessImages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UploadedAt",
                table: "BusinessImages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCover",
                table: "BusinessImages");

            migrationBuilder.DropColumn(
                name: "UploadedAt",
                table: "BusinessImages");

            migrationBuilder.RenameColumn(
                name: "ImageData",
                table: "BusinessImages",
                newName: "ImageContent");
        }
    }
}

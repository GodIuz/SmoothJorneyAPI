using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmoothJorneyAPI.Migrations
{
    /// <inheritdoc />
    public partial class CleaingUpDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Photos_Business_BusinessId",
                table: "Photos");

            migrationBuilder.DropForeignKey(
                name: "FK_TripItems_Business_BusinessId",
                table: "TripItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Trips_Users_UserId",
                table: "Trips");

            migrationBuilder.DropForeignKey(
                name: "FK_Trips_Users_UsersUserId",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Trips_UsersUserId",
                table: "Trips");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Photos",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "UsersUserId",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "TripItems");

            migrationBuilder.RenameTable(
                name: "Photos",
                newName: "BusinessPhoto");

            migrationBuilder.RenameIndex(
                name: "IX_Photos_BusinessId",
                table: "BusinessPhoto",
                newName: "IX_BusinessPhoto_BusinessId");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Trips",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BusinessId",
                table: "TripItems",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BusinessPhoto",
                table: "BusinessPhoto",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessPhoto_Business_BusinessId",
                table: "BusinessPhoto",
                column: "BusinessId",
                principalTable: "Business",
                principalColumn: "BusinessId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TripItems_Business_BusinessId",
                table: "TripItems",
                column: "BusinessId",
                principalTable: "Business",
                principalColumn: "BusinessId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_Users_UserId",
                table: "Trips",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BusinessPhoto_Business_BusinessId",
                table: "BusinessPhoto");

            migrationBuilder.DropForeignKey(
                name: "FK_TripItems_Business_BusinessId",
                table: "TripItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Trips_Users_UserId",
                table: "Trips");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BusinessPhoto",
                table: "BusinessPhoto");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Trips");

            migrationBuilder.RenameTable(
                name: "BusinessPhoto",
                newName: "Photos");

            migrationBuilder.RenameIndex(
                name: "IX_BusinessPhoto_BusinessId",
                table: "Photos",
                newName: "IX_Photos_BusinessId");

            migrationBuilder.AddColumn<int>(
                name: "UsersUserId",
                table: "Trips",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BusinessId",
                table: "TripItems",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "TripItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Photos",
                table: "Photos",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_UsersUserId",
                table: "Trips",
                column: "UsersUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_Business_BusinessId",
                table: "Photos",
                column: "BusinessId",
                principalTable: "Business",
                principalColumn: "BusinessId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TripItems_Business_BusinessId",
                table: "TripItems",
                column: "BusinessId",
                principalTable: "Business",
                principalColumn: "BusinessId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_Users_UserId",
                table: "Trips",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_Users_UsersUserId",
                table: "Trips",
                column: "UsersUserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }
    }
}

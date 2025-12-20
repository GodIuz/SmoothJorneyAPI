using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmoothJorneyAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Business",
                columns: table => new
                {
                    BusinessId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CategoryType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MoodTags = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsHiddenGem = table.Column<bool>(type: "bit", nullable: false),
                    IsSuspectedScam = table.Column<bool>(type: "bit", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    City = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PriceRange = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PriceLevel = table.Column<int>(type: "int", nullable: false),
                    AverageRating = table.Column<double>(type: "float", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CoverImage = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    CoverImageContentType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Business", x => x.BusinessId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordSalt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    EmailVerificationToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResetPasswordToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResetPasswordExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false),
                    LockoutEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerificationToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PasswordResetToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResetTokenExpires = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "BusinessImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImageContent = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BusinessId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessImages_Business_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Business",
                        principalColumn: "BusinessId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BusinessReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsResolved = table.Column<bool>(type: "bit", nullable: false),
                    ShopId = table.Column<int>(type: "int", nullable: false),
                    BusinessId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessReports_Business_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Business",
                        principalColumn: "BusinessId");
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JwtId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isUsed = table.Column<bool>(type: "bit", nullable: false),
                    isRevoked = table.Column<bool>(type: "bit", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Used = table.Column<bool>(type: "bit", nullable: false),
                    Invalidated = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    AverageRating = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Sentiment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BusinessId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_Business_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Business",
                        principalColumn: "BusinessId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reviews_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Trips",
                columns: table => new
                {
                    TripId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TotalBudget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShareToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AverageRating = table.Column<double>(type: "float", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    UsersUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trips", x => x.TripId);
                    table.ForeignKey(
                        name: "FK_Trips_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Trips_Users_UsersUserId",
                        column: x => x.UsersUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "TripCollaborators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TripId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripCollaborators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TripCollaborators_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "TripId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TripCollaborators_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TripItems",
                columns: table => new
                {
                    TripItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TripId = table.Column<int>(type: "int", nullable: false),
                    EstimatedCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BusinessId = table.Column<int>(type: "int", nullable: false),
                    ScheduledTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripItems", x => x.TripItemId);
                    table.ForeignKey(
                        name: "FK_TripItems_Business_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Business",
                        principalColumn: "BusinessId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TripItems_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "TripId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessImages_BusinessId",
                table: "BusinessImages",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessReports_BusinessId",
                table: "BusinessReports",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_BusinessId",
                table: "Reviews",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TripCollaborators_TripId",
                table: "TripCollaborators",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_TripCollaborators_UserId",
                table: "TripCollaborators",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TripItems_BusinessId",
                table: "TripItems",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_TripItems_TripId",
                table: "TripItems",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_UserId",
                table: "Trips",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_UsersUserId",
                table: "Trips",
                column: "UsersUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessImages");

            migrationBuilder.DropTable(
                name: "BusinessReports");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "TripCollaborators");

            migrationBuilder.DropTable(
                name: "TripItems");

            migrationBuilder.DropTable(
                name: "Business");

            migrationBuilder.DropTable(
                name: "Trips");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}

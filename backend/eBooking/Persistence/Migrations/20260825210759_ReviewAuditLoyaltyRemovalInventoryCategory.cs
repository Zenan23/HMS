using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReviewAuditLoyaltyRemovalInventoryCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoyaltyPointsEarned");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "InventoryItems");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Reviews",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedByUserId",
                table: "Reviews",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "Reviews",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RejectedByUserId",
                table: "Reviews",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InventoryItemCategoryId",
                table: "InventoryItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "InventoryItemCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItemCategories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ApprovedByUserId",
                table: "Reviews",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_RejectedByUserId",
                table: "Reviews",
                column: "RejectedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_InventoryItemCategoryId",
                table: "InventoryItems",
                column: "InventoryItemCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItemCategories_Name",
                table: "InventoryItemCategories",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_InventoryItemCategories_InventoryItemCategoryId",
                table: "InventoryItems",
                column: "InventoryItemCategoryId",
                principalTable: "InventoryItemCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Users_ApprovedByUserId",
                table: "Reviews",
                column: "ApprovedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Users_RejectedByUserId",
                table: "Reviews",
                column: "RejectedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_InventoryItemCategories_InventoryItemCategoryId",
                table: "InventoryItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Users_ApprovedByUserId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Users_RejectedByUserId",
                table: "Reviews");

            migrationBuilder.DropTable(
                name: "InventoryItemCategories");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_ApprovedByUserId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_RejectedByUserId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_InventoryItemCategoryId",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "RejectedByUserId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "InventoryItemCategoryId",
                table: "InventoryItems");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "InventoryItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "LoyaltyPointsEarned",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: true),
                    PaymentId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EarnedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    PointsEarned = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyPointsEarned", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltyPointsEarned_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LoyaltyPointsEarned_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LoyaltyPointsEarned_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyPointsEarned_BookingId",
                table: "LoyaltyPointsEarned",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyPointsEarned_PaymentId",
                table: "LoyaltyPointsEarned",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyPointsEarned_UserId",
                table: "LoyaltyPointsEarned",
                column: "UserId");
        }
    }
}

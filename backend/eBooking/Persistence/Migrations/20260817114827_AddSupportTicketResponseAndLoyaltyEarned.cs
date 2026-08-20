using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSupportTicketResponseAndLoyaltyEarned : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminResponse",
                table: "SupportTickets",
                type: "nvarchar(max)",
                maxLength: 5000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RespondedAt",
                table: "SupportTickets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RespondedByUserId",
                table: "SupportTickets",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LoyaltyPointsEarned",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: true),
                    PaymentId = table.Column<int>(type: "int", nullable: true),
                    PointsEarned = table.Column<int>(type: "int", nullable: false),
                    EarnedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
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
                name: "IX_SupportTickets_RespondedByUserId",
                table: "SupportTickets",
                column: "RespondedByUserId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTickets_Users_RespondedByUserId",
                table: "SupportTickets",
                column: "RespondedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupportTickets_Users_RespondedByUserId",
                table: "SupportTickets");

            migrationBuilder.DropTable(
                name: "LoyaltyPointsEarned");

            migrationBuilder.DropIndex(
                name: "IX_SupportTickets_RespondedByUserId",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "AdminResponse",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "RespondedAt",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "RespondedByUserId",
                table: "SupportTickets");
        }
    }
}

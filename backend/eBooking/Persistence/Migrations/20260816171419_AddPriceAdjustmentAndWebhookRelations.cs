using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceAdjustmentAndWebhookRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaymentId",
                table: "ProcessedWebhookEvents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "PriceAdjustments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HotelId",
                table: "PriceAdjustments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedWebhookEvents_PaymentId",
                table: "ProcessedWebhookEvents",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceAdjustments_CreatedByUserId",
                table: "PriceAdjustments",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceAdjustments_HotelId",
                table: "PriceAdjustments",
                column: "HotelId");

            migrationBuilder.AddForeignKey(
                name: "FK_PriceAdjustments_Hotels_HotelId",
                table: "PriceAdjustments",
                column: "HotelId",
                principalTable: "Hotels",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PriceAdjustments_Users_CreatedByUserId",
                table: "PriceAdjustments",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ProcessedWebhookEvents_Payments_PaymentId",
                table: "ProcessedWebhookEvents",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PriceAdjustments_Hotels_HotelId",
                table: "PriceAdjustments");

            migrationBuilder.DropForeignKey(
                name: "FK_PriceAdjustments_Users_CreatedByUserId",
                table: "PriceAdjustments");

            migrationBuilder.DropForeignKey(
                name: "FK_ProcessedWebhookEvents_Payments_PaymentId",
                table: "ProcessedWebhookEvents");

            migrationBuilder.DropIndex(
                name: "IX_ProcessedWebhookEvents_PaymentId",
                table: "ProcessedWebhookEvents");

            migrationBuilder.DropIndex(
                name: "IX_PriceAdjustments_CreatedByUserId",
                table: "PriceAdjustments");

            migrationBuilder.DropIndex(
                name: "IX_PriceAdjustments_HotelId",
                table: "PriceAdjustments");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                table: "ProcessedWebhookEvents");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "PriceAdjustments");

            migrationBuilder.DropColumn(
                name: "HotelId",
                table: "PriceAdjustments");
        }
    }
}

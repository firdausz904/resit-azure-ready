using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Resit.Modules.Receipts.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "receipts");

            migrationBuilder.CreateTable(
                name: "receipts",
                schema: "receipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HouseholdId = table.Column<Guid>(type: "uuid", nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    FileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    StoragePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Merchant = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Total = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    PurchasedOn = table.Column<DateOnly>(type: "date", nullable: true),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MerchantConfidence = table.Column<double>(type: "double precision", nullable: true),
                    TotalConfidence = table.Column<double>(type: "double precision", nullable: true),
                    DateConfidence = table.Column<double>(type: "double precision", nullable: true),
                    RawOcrText = table.Column<string>(type: "text", nullable: true),
                    FailureReason = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_receipts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "upload_batches",
                schema: "receipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HouseholdId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalCount = table.Column<int>(type: "integer", nullable: false),
                    ProcessedCount = table.Column<int>(type: "integer", nullable: false),
                    FailedCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_upload_batches", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_receipts_BatchId",
                schema: "receipts",
                table: "receipts",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_receipts_HouseholdId_PurchasedOn",
                schema: "receipts",
                table: "receipts",
                columns: new[] { "HouseholdId", "PurchasedOn" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "receipts",
                schema: "receipts");

            migrationBuilder.DropTable(
                name: "upload_batches",
                schema: "receipts");
        }
    }
}

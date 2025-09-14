using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace N1coLoyalty.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionMetadataAsJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionMetadata",
                schema: "dbo");

            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                schema: "dbo",
                table: "Transaction",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Metadata",
                schema: "dbo",
                table: "Transaction");

            migrationBuilder.CreateTable(
                name: "TransactionMetadata",
                schema: "dbo",
                columns: table => new
                {
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionMetadata", x => new { x.TransactionId, x.Key });
                    table.ForeignKey(
                        name: "FK_TransactionMetadata_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalSchema: "dbo",
                        principalTable: "Transaction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}

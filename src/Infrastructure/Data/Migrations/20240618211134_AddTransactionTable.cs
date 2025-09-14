using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace N1coLoyalty.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Transaction",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TransactionStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TransactionSubType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaction", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transaction",
                schema: "dbo");
        }
    }
}

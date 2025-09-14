using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace N1coLoyalty.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionIdToUserWalletBalanceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TransactionId",
                schema: "dbo",
                table: "UserWalletBalance",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserWalletBalance_TransactionId",
                schema: "dbo",
                table: "UserWalletBalance",
                column: "TransactionId",
                unique: true,
                filter: "[TransactionId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_UserWalletBalance_Transaction_TransactionId",
                schema: "dbo",
                table: "UserWalletBalance",
                column: "TransactionId",
                principalSchema: "dbo",
                principalTable: "Transaction",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserWalletBalance_Transaction_TransactionId",
                schema: "dbo",
                table: "UserWalletBalance");

            migrationBuilder.DropIndex(
                name: "IX_UserWalletBalance_TransactionId",
                schema: "dbo",
                table: "UserWalletBalance");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                schema: "dbo",
                table: "UserWalletBalance");
        }
    }
}

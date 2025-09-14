using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace N1coLoyalty.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserToTransactionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                schema: "dbo",
                table: "Transaction",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_UserId",
                schema: "dbo",
                table: "Transaction",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_User_UserId",
                schema: "dbo",
                table: "Transaction",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_User_UserId",
                schema: "dbo",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_UserId",
                schema: "dbo",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "dbo",
                table: "Transaction");
        }
    }
}

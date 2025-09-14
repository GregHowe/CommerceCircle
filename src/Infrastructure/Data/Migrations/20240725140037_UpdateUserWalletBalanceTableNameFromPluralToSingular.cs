using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace N1coLoyalty.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserWalletBalanceTableNameFromPluralToSingular : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserWalletBalances_User_UserId",
                schema: "dbo",
                table: "UserWalletBalances");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserWalletBalances",
                schema: "dbo",
                table: "UserWalletBalances");

            migrationBuilder.RenameTable(
                name: "UserWalletBalances",
                schema: "dbo",
                newName: "UserWalletBalance",
                newSchema: "dbo");

            migrationBuilder.RenameIndex(
                name: "IX_UserWalletBalances_UserId",
                schema: "dbo",
                table: "UserWalletBalance",
                newName: "IX_UserWalletBalance_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserWalletBalance",
                schema: "dbo",
                table: "UserWalletBalance",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserWalletBalance_User_UserId",
                schema: "dbo",
                table: "UserWalletBalance",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserWalletBalance_User_UserId",
                schema: "dbo",
                table: "UserWalletBalance");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserWalletBalance",
                schema: "dbo",
                table: "UserWalletBalance");

            migrationBuilder.RenameTable(
                name: "UserWalletBalance",
                schema: "dbo",
                newName: "UserWalletBalances",
                newSchema: "dbo");

            migrationBuilder.RenameIndex(
                name: "IX_UserWalletBalance_UserId",
                schema: "dbo",
                table: "UserWalletBalances",
                newName: "IX_UserWalletBalances_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserWalletBalances",
                schema: "dbo",
                table: "UserWalletBalances",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserWalletBalances_User_UserId",
                schema: "dbo",
                table: "UserWalletBalances",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

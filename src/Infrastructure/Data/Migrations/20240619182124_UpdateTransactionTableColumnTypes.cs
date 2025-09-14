using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace N1coLoyalty.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTransactionTableColumnTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_User_UserId",
                schema: "dbo",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "Date",
                schema: "dbo",
                table: "Transaction");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                schema: "dbo",
                table: "User",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "dbo",
                table: "User",
                newName: "Created");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                schema: "dbo",
                table: "Transaction",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "dbo",
                table: "Transaction",
                newName: "Created");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastSessionDate",
                schema: "dbo",
                table: "User",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastModifiedBy",
                schema: "dbo",
                table: "User",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "dbo",
                table: "User",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModified",
                schema: "dbo",
                table: "User",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                schema: "dbo",
                table: "User",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AlterColumn<string>(
                name: "LastModifiedBy",
                schema: "dbo",
                table: "Transaction",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "dbo",
                table: "Transaction",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModified",
                schema: "dbo",
                table: "Transaction",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                schema: "dbo",
                table: "Transaction",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_User_UserId",
                schema: "dbo",
                table: "Transaction",
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
                name: "FK_Transaction_User_UserId",
                schema: "dbo",
                table: "Transaction");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                schema: "dbo",
                table: "User",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "Created",
                schema: "dbo",
                table: "User",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                schema: "dbo",
                table: "Transaction",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "Created",
                schema: "dbo",
                table: "Transaction",
                newName: "CreatedAt");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastSessionDate",
                schema: "dbo",
                table: "User",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastModifiedBy",
                schema: "dbo",
                table: "User",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "dbo",
                table: "User",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModifiedAt",
                schema: "dbo",
                table: "User",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "dbo",
                table: "User",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "LastModifiedBy",
                schema: "dbo",
                table: "Transaction",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "dbo",
                table: "Transaction",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModifiedAt",
                schema: "dbo",
                table: "Transaction",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "dbo",
                table: "Transaction",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                schema: "dbo",
                table: "Transaction",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_User_UserId",
                schema: "dbo",
                table: "Transaction",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id");
        }
    }
}

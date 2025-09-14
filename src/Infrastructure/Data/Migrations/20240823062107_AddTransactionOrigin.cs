using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace N1coLoyalty.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionOrigin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TransactionOrigin",
                schema: "dbo",
                table: "Transaction",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE dbo.[Transaction] SET TransactionOrigin = 'Game' WHERE TransactionSubType IN ('Retry','Compensation');");
            migrationBuilder.Sql("UPDATE dbo.[Transaction] SET TransactionOrigin = 'Challenge' WHERE CreatedBy = 'system' AND TransactionSubType = 'Point';");
            migrationBuilder.Sql("UPDATE dbo.[Transaction] SET TransactionOrigin = 'Game' WHERE CreatedBy != 'system' AND TransactionSubType = 'Point';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransactionOrigin",
                schema: "dbo",
                table: "Transaction");
        }
    }
}

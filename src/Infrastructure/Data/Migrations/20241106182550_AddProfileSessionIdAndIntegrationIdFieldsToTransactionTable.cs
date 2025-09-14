using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace N1coLoyalty.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileSessionIdAndIntegrationIdFieldsToTransactionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IntegrationId",
                schema: "dbo",
                table: "Transaction",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfileSessionId",
                schema: "dbo",
                table: "Transaction",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IntegrationId",
                schema: "dbo",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "ProfileSessionId",
                schema: "dbo",
                table: "Transaction");
        }
    }
}

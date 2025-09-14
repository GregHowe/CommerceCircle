using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace N1coLoyalty.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class addOnboardingCompletedFieldToUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "OnboardingCompleted",
                schema: "dbo",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);
            
            // Initialize the OnboardingCompleted field for all users who have completed the onboarding process
            migrationBuilder.Sql(@"
                UPDATE U
                SET OnboardingCompleted = 1
                FROM [User] U
                INNER JOIN dbo.[Transaction] T on U.Id = T.UserId
                WHERE T.TransactionOrigin = 'Onboarding';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OnboardingCompleted",
                schema: "dbo",
                table: "User");
        }
    }
}

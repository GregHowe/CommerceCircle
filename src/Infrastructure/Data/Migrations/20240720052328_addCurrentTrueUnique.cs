using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace N1coLoyalty.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class addCurrentTrueUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TermsConditionsInfo_IsCurrent",
                schema: "dbo",
                table: "TermsConditionsInfo",
                column: "IsCurrent",
                unique: true,
                filter: "IsCurrent = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TermsConditionsInfo_IsCurrent",
                schema: "dbo",
                table: "TermsConditionsInfo");
        }
    }
}

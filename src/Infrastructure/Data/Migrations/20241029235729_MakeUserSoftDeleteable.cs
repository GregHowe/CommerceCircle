using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace N1coLoyalty.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeUserSoftDeleteable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "dbo",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "dbo",
                table: "User");
        }
    }
}

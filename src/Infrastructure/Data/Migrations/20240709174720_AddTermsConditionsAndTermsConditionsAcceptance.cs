using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace N1coLoyalty.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTermsConditionsAndTermsConditionsAcceptance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TermsConditionsInfo",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermsConditionsInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TermsConditionsAcceptance",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TermsConditionsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsAccepted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermsConditionsAcceptance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TermsConditionsAcceptance_TermsConditionsInfo_TermsConditionsId",
                        column: x => x.TermsConditionsId,
                        principalSchema: "dbo",
                        principalTable: "TermsConditionsInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TermsConditionsAcceptance_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TermsConditionsAcceptance_TermsConditionsId",
                schema: "dbo",
                table: "TermsConditionsAcceptance",
                column: "TermsConditionsId");

            migrationBuilder.CreateIndex(
                name: "IX_TermsConditionsAcceptance_UserId",
                schema: "dbo",
                table: "TermsConditionsAcceptance",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TermsConditionsAcceptance",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "TermsConditionsInfo",
                schema: "dbo");
        }
    }
}

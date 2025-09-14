using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace N1coLoyalty.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNewTransactionTypesInitialSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE [Transaction] ADD TempSourceId UNIQUEIDENTIFIER NULL;
            ");

            migrationBuilder.Sql(@"
                BEGIN TRANSACTION;

                INSERT INTO [Transaction] (Id, Name, Amount, Description, TransactionStatus, TransactionType, TransactionSubType, TransactionOrigin, UserId, Created, CreatedBy, LastModified, LastModifiedBy, TempSourceId)
                SELECT NEWID(), 'Giro de ruleta', Amount, 'Ruleta n1co', 'Redeemed', 'Debit', 'Point', 'Game', UserId, Created, CreatedBy, LastModified, LastModifiedBy, Id
                FROM UserWalletBalance
                WHERE TransactionId IS NULL AND Reason = 'Costo de evento' AND Action = 'Debit';

                INSERT INTO [Transaction] (Id, Name, Description, Amount, TransactionStatus, TransactionType, TransactionSubType, TransactionOrigin, UserId, Created, CreatedBy, LastModified, LastModifiedBy, TempSourceId)
                SELECT NEWID(), 'Co1ns de bienvenida', 'Premio co1ns', Amount, 'Redeemed', 'Reward', 'Point', 'Onboarding', UserId, Created, CreatedBy, LastModified, LastModifiedBy, Id
                FROM UserWalletBalance
                WHERE TransactionId IS NULL AND Reason = 'Premio Onboarding' AND Action = 'Credit';

                UPDATE uwb
                SET TransactionId = t.Id
                FROM UserWalletBalance uwb
                         INNER JOIN [Transaction] t ON uwb.Id = t.TempSourceId
                WHERE t.TempSourceId IS NOT NULL;

                COMMIT TRANSACTION;
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE [Transaction] DROP COLUMN TempSourceId;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

namespace N1coLoyalty.Application.Common.Interfaces;

public interface IWalletsService
{
    Task<WalletBalanceResponseDto?> GetBalance(string profileIntegrationId);
    
    Task<WalletBalanceResponseDto?> Credit(string profileIntegrationId, decimal amount);
    
    Task<WalletBalanceResponseDto?> CreateWallet(string profileIntegrationId);
    
    Task<WalletBalanceResponseDto?> Debit(string profileIntegrationId, decimal amount);
    
    Task<WalletBalanceResponseDto?> Void(string profileIntegrationId, string transactionId);
}

using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Infrastructure.HttpClients.LoyaltyCore;
using N1coLoyalty.Infrastructure.HttpClients.LoyaltyCore.Models;

namespace N1coLoyalty.Infrastructure.Services.Wallets;

public class WalletsService(
    LoyaltyCoreHttpClient loyaltyCoreHttpClient)
    : IWalletsService
{
    public async Task<WalletBalanceResponseDto?> GetBalance(string profileIntegrationId)
    {
        var walletBalanceResponse = await loyaltyCoreHttpClient.GetBalance(profileIntegrationId);
        return walletBalanceResponse == null ? null : MapWalletBalanceResponseDto(walletBalanceResponse);
    }

    public async Task<WalletBalanceResponseDto?> Credit(string profileIntegrationId, decimal amount)
    {
        var walletBalanceResponse = await loyaltyCoreHttpClient.Credit(profileIntegrationId, amount);
        return walletBalanceResponse == null ? null : MapWalletBalanceResponseDto(walletBalanceResponse);
    }

    public async Task<WalletBalanceResponseDto?> CreateWallet(string profileIntegrationId)
    {
        var walletBalanceResponse = await loyaltyCoreHttpClient.Create(profileIntegrationId);
        return walletBalanceResponse == null ? null : MapWalletBalanceResponseDto(walletBalanceResponse);
    }

    public async Task<WalletBalanceResponseDto?> Debit(string profileIntegrationId, decimal amount)
    {
        var walletBalanceResponse = await loyaltyCoreHttpClient.Debit(profileIntegrationId, amount);
        return walletBalanceResponse == null ? null : MapWalletBalanceResponseDto(walletBalanceResponse);
    }

    public async Task<WalletBalanceResponseDto?> Void(string profileIntegrationId, string transactionId)
    {
        var walletBalanceResponse = await loyaltyCoreHttpClient.Void(profileIntegrationId, transactionId);
        return walletBalanceResponse == null ? null : MapWalletBalanceResponseDto(walletBalanceResponse);   
    }

    private static WalletBalanceResponseDto MapWalletBalanceResponseDto(WalletBalanceResponse walletBalanceResponse)
    {
        return new WalletBalanceResponseDto
        {
            Credit = walletBalanceResponse.Credit,
            Debit = walletBalanceResponse.Debit,
            TransactionId = walletBalanceResponse.TransactionId,
            HistoricalCredit = walletBalanceResponse.HistoricalCredit
        };
    }
}

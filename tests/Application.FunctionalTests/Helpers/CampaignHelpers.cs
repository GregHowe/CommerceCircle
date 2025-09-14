using N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;
using N1coLoyalty.Domain.Enums;

namespace N1coLoyalty.Application.FunctionalTests.Helpers;

using static Testing;
internal static class CampaignHelpers
{
    internal static async Task<LoyaltyCampaignDto> CreateCampaignMock(List<LoyaltyRewardDto>? rewards = null)
    {
        var campaign = new LoyaltyCampaignDto
        {
            Id = "anyId",
            Name = "Test Campaign",
            TotalBudget = 1000,
            ConsumedBudget = 0,
            WalletConversionRate = 0.01m,
            EventCost = 250,
            UserEventFrequency = "Daily",
            UserEventFrequencyLimit = 10,
            ExtraAttemptCost = 350,
            Rewards = rewards ??
       [
                new()
                {
                    IntegrationId = "CASH",
                    Name = "Recargas al instante en tu tarjeta n1co",
                    EffectSubType = EffectSubTypeValue.Cash
                },

                new()
                {
                    IntegrationId = "POINT",
                    Name = "Co1ns para subir de nivel y canjear",
                    EffectSubType = EffectSubTypeValue.Point
                },

                new()
                {
                    IntegrationId = "COMPENSATION",
                    Name = "Gracias por participar en la ruleta",
                    EffectSubType = EffectSubTypeValue.Compensation
                },

                new()
                {
                    IntegrationId = "RETRY",
                    Name = "Giro de nuevo a la ruleta",
                    EffectSubType = EffectSubTypeValue.Retry
                }
       ],
        };
        return await Task.FromResult(campaign);

    }
}

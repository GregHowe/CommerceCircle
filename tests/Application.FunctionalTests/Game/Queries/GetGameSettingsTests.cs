using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;
using N1coLoyalty.Application.Events.Commands;
using N1coLoyalty.Application.FunctionalTests.Helpers;
using N1coLoyalty.Application.Game.Queries;
using N1coLoyalty.Domain.Enums;

namespace N1coLoyalty.Application.FunctionalTests.Game.Queries;

using static Testing;
using static TermsConditionsHelpers;

public class GetGameSettingsTests : BaseTestFixture
{
    private Mock<ILoyaltyEngine> _loyaltyEngine = new();
    
    [SetUp]
    public async Task SetUp()
    {
        await CreateTermsConditions();
    }

    [Test]
    public async Task ShouldReturnGameSettings()
    {
        // Act #1
        var query = new GetGameSettingsQuery();
        
        // Assert #1
        var result = await SendAsync(query);
        
        result.Should().NotBeNull();

        result.AttemptCost.Should().Be(250);
        result.ExtraAttemptCost.Should().Be(350);
        result.RemainingAttempts.Should().Be(3);
        result.UnredeemedFreeAttempts.Should().Be(0);
        result.AttemptsLimit.Should().Be(3);
        
        // Arrange #2
        var walletsService = GetServiceMock<IWalletsService>();
        walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 10000, Debit = 0});

        walletsService.Setup(x => x.Debit(It.IsAny<string>(), 250))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 10000, Debit = 0});

        walletsService.Setup(x => x.Credit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 10000, Debit = 0});
        
        // spin the wheel 3 times
        for (var i = 0; i < 3; i++)
        {
            var gameCommand = new ProcessEventCommand
            {
                EventType = EventTypeValue.PlayGame,
            };
            await SendAsync(gameCommand);
        }
        
        // Act #2
        var result2 = await SendAsync(query);
        
        // Assert #2
        result2.Should().NotBeNull();
        result2.RemainingAttempts.Should().Be(0);
        result2.AttemptCost.Should().Be(250);
        result2.AttemptsLimit.Should().Be(3);
    }
    
    [Test]
    public async Task ShouldReturnGameSettingsForUnlimitedCampaign()
    {
        _loyaltyEngine = GetServiceMock<ILoyaltyEngine>();
        _loyaltyEngine.Setup(x => x.GetCampaign(It.IsAny<string>(),It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>()))
            .ReturnsAsync(new LoyaltyCampaignDto
            {
                Id = "id",
                Name = "Test Campaign",
                EventCost = 250,
                ConsumedBudget = 0,
                TotalBudget = 1000,
                UserEventFrequency = null,
                UserEventFrequencyLimit = null,
                WalletConversionRate = 0.01m,
                Rewards =
                [
                    new LoyaltyRewardDto
                    {
                        Id = Guid.NewGuid(),
                        IntegrationId = "CASH",
                        Name = "Recargas al instante en tu tarjeta n1co",
                        EffectSubType = EffectSubTypeValue.Cash
                    },
                    new LoyaltyRewardDto
                    {
                        Id = Guid.NewGuid(), 
                        IntegrationId = "POINT", 
                        Name = "Co1ns para subir de nivel y canjear",
                        EffectSubType = EffectSubTypeValue.Point
                    },
                    new LoyaltyRewardDto
                    {
                        Id = Guid.NewGuid(),
                        IntegrationId = "COMPENSATION",
                        Name = "Gracias por participar en la ruleta",
                        EffectSubType = EffectSubTypeValue.Compensation
                    },
                    new LoyaltyRewardDto
                    {
                        Id = Guid.NewGuid(), 
                        IntegrationId = "RETRY",
                        Name = "Giro de nuevo a la ruleta",
                        EffectSubType = EffectSubTypeValue.Retry
                    }
                ]
            });
        
        // Act #1
        var query = new GetGameSettingsQuery();
        
        // Assert #1
        var result = await SendAsync(query);
        
        result.Should().NotBeNull();

        result.AttemptCost.Should().Be(250);
        result.RemainingAttempts.Should().Be(null);
        result.UnredeemedFreeAttempts.Should().Be(0);
    }
}

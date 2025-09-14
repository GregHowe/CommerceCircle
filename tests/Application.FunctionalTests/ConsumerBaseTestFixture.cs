using MassTransit;
using N1coLoyalty.Application.Common.Constants;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;
using N1coLoyalty.Application.IntegrationEvents;
using N1coLoyalty.Application.IssuingEvents;
using N1coLoyalty.Application.NotificationEvents;
using N1coLoyalty.Domain.Enums;

namespace N1coLoyalty.Application.FunctionalTests;

using static Testing;
public class ConsumerBaseTestFixture
{
    [SetUp]
    public async Task TestSetUp()
    {
        var integrationEventBusMock = GetServiceMock<IIntegrationEventsBus>();
        integrationEventBusMock.Invocations.Clear();
        
        var notificationEventBusMock = GetServiceMock<INotificationEventBus>();
        notificationEventBusMock.Invocations.Clear();
        
        var issuingEventBusMock = GetServiceMock<IIssuingEventsBus>();
        issuingEventBusMock.Invocations.Clear();
        
        var dateTime = GetServiceMock<IDateTime>();
        dateTime.SetupGet(x => x.Now).Returns(DateTime.Now);

        var currentUserService = GetServiceMock<IUser>();
        currentUserService.Setup(u => u.ExternalId).Returns("anyIdUser");

        var loyaltyEngine = GetServiceMock<ILoyaltyEngine>();
        loyaltyEngine.Invocations.Clear();

        var profileDto = new LoyaltyProfileDto
        {
            IntegrationId = "anyIdUser",
            PhoneNumber = "123456789",
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@email.com",
            Referral = new LoyaltyReferralDto { Code = "REFERRAL-CODE", IsActive = false, },
            LoyaltyPrograms =
            [
                new LoyaltyProgramDto
                {
                    Id = "n1co-loyalty",
                    IntegrationId = "n1co-loyalty",
                    Name = "Loyalty Program",
                    Description = "Description",
                    Tiers =
                    [
                        new LoyaltyTierDto
                        {
                            Id = Guid.Empty.ToString(),
                            Name = "Tier1",
                            IsCurrent = true,
                            IsLocked = false,
                            PointThreshold = 0,
                            PointsToNextTier = 1000,
                            MotivationalMessage = "message",
                            Challenges =
                            [
                                new LoyaltyProfileChallengeDto
                                {
                                    Id = Guid.Empty.ToString(),
                                    Name = "Challenge1",
                                    Description = "Description",
                                    Target = 1,
                                    TargetProgress = 0,
                                    Type = ChallengeTypeValue.OutgoingTransfer,
                                    EffectValue = 200,
                                    EffectType = EffectTypeValue.Reward,
                                    EffectSubType = EffectSubTypeValue.Point,
                                    Stores =
                                    [
                                        new LoyaltyStoreDto
                                        {
                                            Id = Guid.Empty.ToString(),
                                            Name = "Store1",
                                            Description = "Description",
                                            ImageUrl = "https://test.com/image.jpg",
                                        }
                                    ]
                                }
                            ]
                        }
                    ]
                }
            ]
        };
        loyaltyEngine.Setup(x => x.GetOrCreateProfile(It.IsAny<string>(), It.IsAny<LoyaltyCreateProfileInput>()))
            .ReturnsAsync(new ProfileCreationDto { Success = true, Message = "Profile created", Profile = profileDto });
        
        loyaltyEngine.Setup(x => x.GetProfile(It.IsAny<string>())).ReturnsAsync(profileDto);

        loyaltyEngine.Setup(x => x.GetCampaign(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>()))
            .ReturnsAsync(new LoyaltyCampaignDto
            {
                Id = "id",
                Name = "Test Campaign",
                EventCost = 250,
                ConsumedBudget = 0,
                TotalBudget = 1000,
                UserEventFrequency = "Daily",
                UserEventFrequencyLimit = 3,
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

        loyaltyEngine.Setup(x => x.ProcessEventAsync(It.IsAny<ProcessEventInputDto>()))
            .ReturnsAsync(new ProcessEventResponseDto
            {
                Success = true,
                Message = "Event processed",
                Code = "EVENT-PROCESSED",
            });
        
        loyaltyEngine.Setup(x => x.VoidSession(It.IsAny<VoidSessionInputDto>()))
            .ReturnsAsync(new VoidSessionResponseDto());

        loyaltyEngine.Setup(x => x.GetRewardByProbability(It.IsAny<RewardByProbabilityInputDto>()))
            .ReturnsAsync(new RewardByProbabilityResponseDto
            {
                Success = true,
                Message = "Event processed",
                Code = "EVENT-PROCESSED",
                Effect = new LoyaltyEffectDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Co1ns para subir de nivel y canjear",
                    Type = EffectTypeValue.Reward,
                    Status = EffectStatus.Completed,
                    Action = new LoyaltyEffectActionDto
                    {
                        Type = "AddPoints",
                        Amount = 200,
                        Metadata = new Dictionary<string, string>
                        {
                            { "transactionId", "t-id" }
                        }
                    },
                    CampaignId = "campaign-id"
                }
            });

        await ResetState();
    }

    /// <summary>
    /// Publishes a message
    /// </summary>
    /// <param name="message">The message that should be published.</param>
    /// <typeparam name="TMessage">The message that should be published.</typeparam>
    public static async Task PublishMessage<TMessage>(object message)
        where TMessage : class
    {
        await Harness.Bus.Publish<TMessage>(message);
    }

    /// <summary>
    /// Sends a message
    /// </summary>
    /// <param name="message">The message that should be sent.</param>
    /// <typeparam name="TMessage">The message that should be sent.</typeparam>
    public static async Task SendMessage<TMessage>(object message)
        where TMessage : class
    {
        await Harness.Bus.Send<TMessage>(message);
    }

    /// <summary>
    /// Confirm that a message has been published.
    /// </summary>
    /// <typeparam name="TMessage">The message that should be published.</typeparam>
    /// <returns>Return true if a message of the given type has been published.</returns>
    public static async Task<bool> IsPublished<TMessage>()
        where TMessage : class
    {
        return await Harness.Published.Any<TMessage>();
    }

    /// <summary>
    /// Confirm that a message has been consumed.
    /// </summary>
    /// <typeparam name="TMessage">The message that should be consumed.</typeparam>
    /// <returns>Return true if a message of the given type has been consumed.</returns>
    public static async Task<bool> IsConsumed<TMessage>()
        where TMessage : class
    {
        return await Harness.Consumed.Any<TMessage>();
    }

    /// <summary>
    /// The desired consumer consumed the message.
    /// </summary>
    /// <typeparam name="TMessage">The message that should be consumed.</typeparam>
    /// <typeparam name="TConsumedBy">The consumer of the message.</typeparam>
    /// <returns>Return true if a message of the given type has been consumed by the given consumer.</returns>
    public static async Task<bool> IsConsumed<TMessage, TConsumedBy>()
        where TMessage : class
        where TConsumedBy : class, IConsumer
    {
        var consumerHarness = Harness.GetConsumerHarness<TConsumedBy>();
        await Task.Delay(1300);
        return await consumerHarness.Consumed.Any<TMessage>();
    }

    /// <summary>
    /// Confirm if there was a fault when publishing.
    /// </summary>
    /// <typeparam name="TMessage">The message that should be published.</typeparam>
    /// <returns>Return true if there was a fault for a message of the given type when published.</returns>
    public static async Task<bool> IsFaultyPublished<TMessage>()
        where TMessage : class
    {
        return await Harness.Published.Any<Fault<TMessage>>();
    }

    /// <summary>
    /// Confirm if there was a fault when consuming.
    /// </summary>
    /// <typeparam name="TMessage">The message that should be consumed.</typeparam>
    /// <returns>Return true if there was a fault for a message of the given type when consumed.</returns>
    public static async Task<bool> IsFaultyConsumed<TMessage>()
        where TMessage : class
    {
        return await Harness.Consumed.Any<Fault<TMessage>>();
    }

    public static TMessage? GetLastPublishedMessage<TMessage>()
        where TMessage : class
    {
        var messages = Harness.Published.Select<TMessage>();
        return messages.LastOrDefault()?.Context.Message;
    }
}

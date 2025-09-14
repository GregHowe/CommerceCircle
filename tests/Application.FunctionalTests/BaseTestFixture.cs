using N1coLoyalty.Application.Common.Constants;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Services.CashBack;
using N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;
using N1coLoyalty.Application.Common.Security;
using N1coLoyalty.Application.IntegrationEvents;
using N1coLoyalty.Application.IssuingEvents;
using N1coLoyalty.Application.NotificationEvents;
using N1coLoyalty.Domain.Enums;

namespace N1coLoyalty.Application.FunctionalTests;

using static Testing;

[TestFixture]
public abstract class BaseTestFixture
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
        currentUserService.Setup(u => u.Permissions).Returns(Permission.GetAllPermissions);

        var loyaltyEngine = GetServiceMock<ILoyaltyEngine>();

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
            ],
            Balance = new WalletBalanceResponseDto()
            {
                Credit = 0,
                Debit = 0,
                HistoricalCredit = 0
            }
        };
        loyaltyEngine.Setup(x => x.GetOrCreateProfile(It.IsAny<string>(), It.IsAny<LoyaltyCreateProfileInput>()))
            .ReturnsAsync(new ProfileCreationDto { Success = true, Message = "Profile created", Profile = profileDto });

        loyaltyEngine.Setup(x => x.GetProfile(It.IsAny<string>())).ReturnsAsync(profileDto);

        loyaltyEngine.Setup(x => x.GetCampaign(It.IsAny<string>(),It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>()))
            .ReturnsAsync(new LoyaltyCampaignDto
            {
                Id = "id",
                Name = "Test Campaign",
                EventCost = 250,
                ExtraAttemptCost = 350,
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
                    new LoyaltyRewardDto { Id = Guid.NewGuid(), IntegrationId = "RETRY", Name = "Giro de nuevo a la ruleta", }
                ]
            });
        
        loyaltyEngine.Setup(x => x.ProcessEventAsync(It.IsAny<ProcessEventInputDto>()))
            .ReturnsAsync(new ProcessEventResponseDto
            {
                Success = true,
                Message = "Event processed",
                Code = "EVENT-PROCESSED",
            });
        
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
                    CampaignId = "campaign-id",
                    Reward = new LoyaltyRewardDto()
                    {
                        Id = Guid.NewGuid(),
                        IntegrationId = "POINT",
                        Name = "Has acumulado co1ns",
                        EffectSubType = EffectSubTypeValue.Point
                    }
                }
            });
        
        loyaltyEngine.Setup(x=>x.VoidSession(It.IsAny<VoidSessionInputDto>()))
            .ReturnsAsync(new VoidSessionResponseDto
            {
                Success = true,
                Message = "Session voided",
                Code = "OK",
                Data = new VoidSessionDataResponseDto()
                {
                    OriginalProfileSession = new LoyaltyProfileSessionDto()
                    {
                        Id = Guid.NewGuid(),
                        Status = "Closed",
                        Event = new LoyaltyEventDto()
                        {
                            EventType = "EXPENSE",
                            Attributes = new Dictionary<string, object>()
                            {
                                { "transactionId", Guid.NewGuid().ToString() },
                                {"transactionAmount", 500}
                            }
                        },
                        Effects =
                        [
                            new LoyaltyEffectDto
                            {
                                Id = Guid.NewGuid().ToString(),
                                Name = "Co1ns para subir de nivel y canjear",
                                Type = EffectTypeValue.Reward,
                                Status = EffectStatus.Completed,
                                Action = new LoyaltyEffectActionDto
                                {
                                    Type = "AddPoints",
                                    Amount = 500,
                                    Metadata = new Dictionary<string, string> { { "transactionId", "t-id" } }
                                },
                                CampaignId = "campaign-id",
                                Reward = new LoyaltyRewardDto()
                                {
                                    Id = Guid.NewGuid(),
                                    IntegrationId = "POINT",
                                    Name = "Has acumulado co1ns",
                                    EffectSubType = EffectSubTypeValue.Point
                                }
                            }
                        ],
                        Profile = new LoyaltyProfileDto()
                        {
                            IntegrationId = "anyIdUser",
                            PhoneNumber = "123456789",
                            FirstName = "John",
                            LastName = "Doe",
                            Email = "john.doe@email.com",
                        }
                    },
                    VoidProfileSession = new LoyaltyProfileSessionDto()
                    {
                        Id = Guid.NewGuid(),
                        Status = "Closed",
                        Event = new LoyaltyEventDto()
                        {
                            EventType = "EXPENSE",
                            Attributes = new Dictionary<string, object>()
                            {
                                { "transactionId", Guid.NewGuid().ToString() },
                                {"transactionAmount", 500}
                            }
                        },
                        Effects =
                        [
                            new LoyaltyEffectDto()
                            {
                                Id = Guid.NewGuid().ToString(),
                                Name = "Co1ns para subir de nivel y canjear",
                                Type = EffectTypeValue.Reward,
                                Status = EffectStatus.Voided,
                                Action = new LoyaltyEffectActionDto
                                {
                                    Type = "AddPoints",
                                    Amount = 500,
                                    Metadata = new Dictionary<string, string> { { "transactionId", "t-id" } }
                                },
                                CampaignId = "campaign-id",
                                Reward = new LoyaltyRewardDto()
                                {
                                    Id = Guid.NewGuid(),
                                    IntegrationId = "POINT",
                                    Name = "Has acumulado co1ns",
                                    EffectSubType = EffectSubTypeValue.Point
                                }
                            }
                        ],
                        Profile = new LoyaltyProfileDto()
                        {
                            IntegrationId = "anyIdUser",
                            PhoneNumber = "123456789",
                            FirstName = "John",
                            LastName = "Doe",
                            Email = "john.doe@email.com",
                        }
                    }
                }
            });
        
        var cashbackService = GetServiceMock<ICashBackService>();
        cashbackService.Setup(x=>x.ApplyCashBack(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new ApplyCashBackDto()
            {
                Success = true,
                Message = "Cashback applied",
                Code = "CASHBACK-APPLIED",
                CashBackTransaction = new CashBackTransactionDto()
                {
                    Id = "appliedCashBackTransactionId",
                    OriginTransactionId = "cashBackTransactionId",
                    Amount = 100
                }
            });
        cashbackService.Invocations.Clear();

        await ResetState();
    }
}

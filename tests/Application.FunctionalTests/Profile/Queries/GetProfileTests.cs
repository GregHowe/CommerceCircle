using N1coLoyalty.Application.Common.Constants;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;
using N1coLoyalty.Application.Profile.Queries;
using N1coLoyalty.Domain.Entities;
using N1coLoyalty.Domain.Enums;

namespace N1coLoyalty.Application.FunctionalTests.Profile.Queries;
using static Testing;

public class GetProfileTests : BaseTestFixture
{
    private Mock<IWalletsService> _walletsService = new();
    private Mock<ILoyaltyEngine> _loyaltyEngine = new();
    private const int HistoricalCredit = 2;

    [SetUp]
    public void Setup()
    {
        _walletsService = GetServiceMock<IWalletsService>();
        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto
            {
                Credit = HistoricalCredit,
                Debit = 0,
                HistoricalCredit = HistoricalCredit,
            });
    }

    [Test]
    public async Task ShouldCreateAProfileIfProfileDoesntExists()
    {
        // Arrange
        var loyaltyStoreDto = new LoyaltyStoreDto
        {
            Id = Guid.Empty.ToString(),
            Name = "Store1",
            Description = "Description",
            ImageUrl = "https://test.com/image.jpg",
        };
        var loyaltyProfileChallengeDto = new LoyaltyProfileChallengeDto
        {
            Id = Guid.Empty.ToString(),
            Name = "Challenge1",
            Description = "Description",
            Target = 1,
            TargetProgress = 0,
            Type = ChallengeTypeValue.Expense,
            EffectValue = 200,
            EffectType = EffectTypeValue.Reward,
            EffectSubType = EffectSubTypeValue.Point,
            Stores = 
            [
                loyaltyStoreDto
            ]
        };
        var profileMock = new LoyaltyProfileDto
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
                            PointThreshold = 0,
                            PointsToNextTier = 998,
                            MotivationalMessage = "message",
                            IsLocked = false,
                            Challenges = [
                                loyaltyProfileChallengeDto
                            ],
                            Benefits = [
                                new LoyaltyBenefitDto()
                                {
                                    Id = Guid.Empty.ToString(),
                                    Description = "N1co shop disponible",
                                    Type = BenefitTypeValue.ShopAccess,
                                }
                            ]
                        },
                        new LoyaltyTierDto
                        {
                            Id = Guid.Empty.ToString(),
                            Name = "Tier2",
                            IsCurrent = false,
                            PointThreshold = 1000,
                            MotivationalMessage = "message",
                            IsLocked = false,
                            Challenges =
                            [
                                new LoyaltyProfileChallengeDto
                                {
                                    Id = Guid.Empty.ToString(),
                                    Name = "Challenge1",
                                    Description = "Description",
                                    Target = 1,
                                    TargetProgress = 0,
                                    Type = ChallengeTypeValue.Expense,
                                    EffectValue = 200,
                                    EffectType = EffectTypeValue.Reward,
                                    EffectSubType = EffectSubTypeValue.Point,
                                }
                            ],
                        },
                        new LoyaltyTierDto
                        {
                            Id = Guid.Empty.ToString(),
                            Name = "Tier3",
                            IsCurrent = false,
                            IsLocked = false,
                            PointThreshold = 2000,
                            MotivationalMessage = "message",
                            Challenges = [],
                        }
                    ]
                }
            ]
        };

        _loyaltyEngine = GetServiceMock<ILoyaltyEngine>();
        profileMock.Referral.IsActive = true;
        _loyaltyEngine.Setup(x => x.GetOrCreateProfile(It.IsAny<string>(), It.IsAny<LoyaltyCreateProfileInput>()))
            .ReturnsAsync(new ProfileCreationDto()
            {
                Success = true,
                Message = "Profile created",
                Profile = profileMock
            });

        var query = new GetProfileQuery();

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.LoyaltyProgram?.Tiers.Count.Should().Be(3);
        result.LoyaltyProgram?.Tiers.First().Name.Should().Be("Tier1");
        result.LoyaltyProgram?.Tiers.First().Challenges.Count.Should().Be(1);
        result.LoyaltyProgram?.Tiers.First().Challenges.First().Name.Should().Be("Challenge1");
        result.LoyaltyProgram?.Tiers.First().Benefits.Count.Should().Be(1);
        result.LoyaltyProgram?.Tiers.First().Benefits.First().Type.Should().Be(BenefitTypeValue.ShopAccess);
        result.LoyaltyProgram?.Tiers.First().Benefits.First().Description.Should().Be("N1co shop disponible");

        var currentTier = result.LoyaltyProgram?.Tiers.First(t => t.Status == LevelStatusValue.InProgress);
        if (currentTier is not null)
        {
            currentTier.Challenges.Should().HaveCount(1);
            var firstChallenge = currentTier.Challenges.First();
            firstChallenge.Id.Should().Be(new Guid(loyaltyProfileChallengeDto.Id));
            firstChallenge.Name.Should().Be(loyaltyProfileChallengeDto.Name);
            firstChallenge.Description.Should().Be(loyaltyProfileChallengeDto.Description);
            firstChallenge.Type.Should().Be(loyaltyProfileChallengeDto.Type);
            firstChallenge.Status.Should().Be(ChallengeStatusValue.Pending);
            firstChallenge.Target.Should().Be(loyaltyProfileChallengeDto.Target);
            firstChallenge.TargetProgress.Should().Be(loyaltyProfileChallengeDto.TargetProgress);
            firstChallenge.Stores.Should().HaveCount(loyaltyProfileChallengeDto.Stores.Count);
            
            var store = firstChallenge.Stores.First();
            store.Id.Should().Be(new Guid(loyaltyStoreDto.Id));
            store.Name.Should().Be(loyaltyStoreDto.Name);
            store.Description.Should().Be(loyaltyStoreDto.Description);
            store.ImageUrl.Should().Be(loyaltyStoreDto.ImageUrl);

            currentTier.PointThreshold.Should().Be(0);
            var nextTier = result.LoyaltyProgram?.Tiers.OrderBy(t => t.PointThreshold)
                .First(t => t.PointThreshold > currentTier.PointThreshold);
            if (nextTier is not null)
                currentTier.PointsToNextTier.Should().Be(Convert.ToInt32(nextTier.PointThreshold - HistoricalCredit));
        }

        result.Referral.Should().NotBeNull();
        result.Referral?.Code.Should().Be(ReferralCode);
        result.Referral?.IsActive.Should().BeFalse();
    }
    
    [Test]
    public async Task ShouldCreateUser()
    {
        // Arrange
        _walletsService = GetServiceMock<IWalletsService>();
        
        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0, HistoricalCredit = 500});
        
        _walletsService.Setup(x => x.Credit(It.IsAny<string>(),It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0, HistoricalCredit = 500});
        
        _walletsService.Setup(x => x.CreateWallet(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0, HistoricalCredit = 500});
        
        //Act
        var profileDto = await SendAsync(new GetProfileQuery());

        //Assert
        profileDto.Should().NotBeNull();
        profileDto.Balance.AccumulatedCoins.Should().Be(500);
        profileDto.Balance.AvailableCoins.Should().Be(500);
        profileDto.IsNew.Should().BeTrue();
        
        //Check wallet balance
        var walletBalanceList = await ToListAsync<UserWalletBalance>(x => true);
        walletBalanceList.Should().HaveCount(2);
        walletBalanceList.Select(x => x.Action).Should().Contain([WalletActionValue.Create, WalletActionValue.Credit]);
        
        //Check transaction
        var transactionList = await ToListAsync<Transaction>(x => true);
        transactionList.Count.Should().Be(1);
        
        var transaction = transactionList[0];
        transaction.Name.Should().Be(TransactionName.OnboardingReward);
        transaction.Amount.Should().Be(500);
        transaction.TransactionType.Should().Be(EffectTypeValue.Reward);
        transaction.TransactionSubType.Should().Be(EffectSubTypeValue.Point);
        transaction.TransactionStatus.Should().Be(TransactionStatusValue.Redeemed);
        transaction.TransactionOrigin.Should().Be(TransactionOriginValue.Onboarding);
        
        //Check user
        var user = await FirstOrDefaultAsync<User>(u=> u.ExternalUserId == profileDto.IntegrationId);
        user.Should().NotBeNull();
        user!.OnboardingCompleted.Should().BeTrue();
    }
    
    [Test]
    public async Task ShouldGetProfileWhenUserAlreadyExists()
    {
        // Arrange
        _walletsService = GetServiceMock<IWalletsService>();
        
        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0});
        
        _walletsService.Setup(x => x.Credit(It.IsAny<string>(),It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0});
        
        _walletsService.Setup(x => x.CreateWallet(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0});
        
        await SendAsync(new GetProfileQuery());
        
        // Arrange
        var profileMock = new LoyaltyProfileDto
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
                            PointsToNextTier = 998,
                            MotivationalMessage = "message",
                            Challenges = [
                                new LoyaltyProfileChallengeDto
                                {
                                    Id = Guid.Empty.ToString(),
                                    Name = "Challenge1",
                                    Description = "Description",
                                    Target = 1,
                                    TargetProgress = 1,
                                    Type = ChallengeTypeValue.Referral,
                                    EffectValue = 200,
                                    EffectType = EffectTypeValue.Reward,
                                    EffectSubType = EffectSubTypeValue.Point,
                                }
                            ],
                        }
                    ]
                }
            ],
            Balance = new WalletBalanceResponseDto()
            {
                Credit = 500,
                Debit = 0,
                HistoricalCredit = 500
            }
        };

        _loyaltyEngine = GetServiceMock<ILoyaltyEngine>();
        profileMock.Referral.IsActive = true;
        _loyaltyEngine.Setup(x => x.GetOrCreateProfile(It.IsAny<string>(),It.IsAny<LoyaltyCreateProfileInput>()))
            .ReturnsAsync(new ProfileCreationDto()
            {
                Success = true,
                Message = "Profile created",
                Profile = profileMock
            });

        // Act
        var profileDto = await SendAsync(new GetProfileQuery());
        profileDto.IsNew.Should().BeFalse();
        
        //Check wallet balance
        var walletBalanceList = await ToListAsync<UserWalletBalance>(x => true);
        walletBalanceList.Should().HaveCount(2);
        walletBalanceList.Select(x => x.Action).Should().Contain([WalletActionValue.Create, WalletActionValue.Credit]);
        
        var onboardingTransactions = await ToListAsync<Transaction>(x => x.TransactionOrigin == TransactionOriginValue.Onboarding);
        onboardingTransactions.Should().HaveCount(1);
        
        profileDto.Should().NotBeNull();
        profileDto.Balance.AccumulatedCoins.Should().Be(500);
        profileDto.Balance.AvailableCoins.Should().Be(500);
        
        //Check user
        var users = await ToListAsync<User>(u=>true);
        users.Should().HaveCount(1);
    }
    
    [Test]
    public async Task ShouldGetProfileButUserOnboardingCompletedIsFalseWhenWalletCreationFails()
    {
        // Arrange
        _walletsService = GetServiceMock<IWalletsService>();
        
        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0, HistoricalCredit = 0});
        
        _walletsService.Setup(x => x.Credit(It.IsAny<string>(),It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0, HistoricalCredit = 0});
        
        _walletsService.Setup(x => x.CreateWallet(It.IsAny<string>()))
            .ReturnsAsync(() => null);
        
        // Act
        var profileDto = await SendAsync(new GetProfileQuery());

        profileDto.Should().NotBeNull();
        profileDto.IsNew.Should().BeFalse();
        
        //Check user
        var user = await FirstOrDefaultAsync<User>(u=> u.ExternalUserId == profileDto.IntegrationId);
        user.Should().NotBeNull();
        user!.OnboardingCompleted.Should().BeFalse();
        
        //Check wallet balance
        var walletBalanceList = await ToListAsync<UserWalletBalance>(x => true);
        walletBalanceList.Count.Should().Be(1);
        walletBalanceList[0].Action.Should().Be(WalletActionValue.Credit);
        
        var transactionList = await ToListAsync<Transaction>(x => true);
        transactionList.Count.Should().Be(1);
    }
    
    [Test]
    public async Task ShouldGetProfileButUserOnboardingCompletedIsFalseWhenCreditFails()
    {
        // Arrange
        _walletsService = GetServiceMock<IWalletsService>();
        
        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0 , HistoricalCredit = 0});
        
        _walletsService.Setup(x => x.Credit(It.IsAny<string>(),It.IsAny<decimal>()))
            .ReturnsAsync(() => null);
        
        _walletsService.Setup(x => x.CreateWallet(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0 , HistoricalCredit = 0});
        
        // Act
        var profileDto = await SendAsync(new GetProfileQuery());
        profileDto.Should().NotBeNull();
        profileDto.IsNew.Should().BeFalse();
        
        //Check wallet balance
        var walletBalanceList = await ToListAsync<UserWalletBalance>(x => true);
        walletBalanceList.Should().HaveCount(1);

        var firstWalletBalance = walletBalanceList[0];
        firstWalletBalance.Action.Should().Be(WalletActionValue.Create);
        
        var transactionList = await ToListAsync<Transaction>(x => true);
        transactionList.Count.Should().Be(0);
    }

    [Test]
    public async Task Should_Fail_When_Profile_Creation_Fails()
    {
        // Arrange
        _loyaltyEngine = GetServiceMock<ILoyaltyEngine>();

        _loyaltyEngine.Setup(x => x.GetProfile(It.IsAny<string>()))
            .ReturnsAsync(() => null);

        _loyaltyEngine.Setup(x => x.GetOrCreateProfile(It.IsAny<string>(), It.IsAny<LoyaltyCreateProfileInput>()))
            .ReturnsAsync(new ProfileCreationDto { Success = false, Message = "Error al crear el perfil" });

        // Act
        var response = await SendAsync(new GetProfileQuery());

        response.Should().BeNull();

        //Check wallet balance
        var walletBalanceList = await ToListAsync<UserWalletBalance>(x => true);
        walletBalanceList.Count.Should().Be(0);
        
        var transactionList = await ToListAsync<Transaction>(x => true);
        transactionList.Count.Should().Be(0);
        
        //Check user
        var users = await ToListAsync<User>(u=>true);
        users.Should().HaveCount(1);
}

    [Test]
    public async Task ShouldReturnAProfileIfProfileAlreadyExists()
    {
        // Arrange
        var profileMock = new LoyaltyProfileDto
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
                            IsCurrent = false,
                            IsLocked = false,
                            PointThreshold = 0,
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
                                    Type = ChallengeTypeValue.Referral,
                                    EffectValue = 200,
                                    EffectType = EffectTypeValue.Reward,
                                    EffectSubType = EffectSubTypeValue.Point,
                                }
                            ],
                            Benefits = [
                                new LoyaltyBenefitDto()
                                {
                                    Id = Guid.Empty.ToString(),
                                    Description = "N1co shop disponible",
                                    Type = BenefitTypeValue.ShopAccess,
                                },
                                new LoyaltyBenefitDto()
                                {
                                    Id = Guid.Empty.ToString(),
                                    Description = "Premios de $0.50 a $15",
                                    Type = BenefitTypeValue.Rewards,
                                }
                            ]
                        },
                        new LoyaltyTierDto
                        {
                            Id = Guid.Empty.ToString(),
                            Name = "Tier2",
                            IsCurrent = true,
                            IsLocked = false,
                            PointThreshold = 1000,
                            PointsToNextTier = 1000,
                            MotivationalMessage = "message",
                            Challenges =
                            [
                            ],
                        },
                        new LoyaltyTierDto
                        {
                            Id = Guid.Empty.ToString(),
                            Name = "Tier3",
                            IsCurrent = false,
                            IsLocked = false,
                            PointThreshold = 2000,
                            MotivationalMessage = "message",
                            Challenges =
                            [
                            ],
                        }
                    ]
                }
            ]
        };

        _loyaltyEngine = GetServiceMock<ILoyaltyEngine>();
        profileMock.Referral.IsActive = true;
        _loyaltyEngine.Setup(x => x.GetOrCreateProfile(It.IsAny<string>(), It.IsAny<LoyaltyCreateProfileInput>()))
            .ReturnsAsync(new ProfileCreationDto()
            {
                Success = true,
                Message = "Profile created",
                Profile = profileMock
            });

        var query = new GetProfileQuery();

        var result = await SendAsync(query);

        result.Should().NotBeNull();

        result.LoyaltyProgram?.Tiers.Count.Should().Be(3);
        var firstTierDto = result.LoyaltyProgram?.Tiers.First();
        firstTierDto?.Name.Should().Be("Tier1");
        firstTierDto?.Challenges.Count.Should().Be(1);
        firstTierDto?.Challenges.First().Name.Should().Be("Challenge1");
        firstTierDto?.Challenges.First().Type.Should().Be(ChallengeTypeValue.Referral);
        firstTierDto?.Challenges.First().Effect?.Amount.Should().Be(200);
        firstTierDto?.Challenges.First().Effect?.Type.Should().Be(EffectTypeValue.Reward);
        firstTierDto?.Challenges.First().Effect?.SubType.Should().Be(EffectSubTypeValue.Point);
        firstTierDto?.Benefits.Count.Should().Be(2);
        firstTierDto?.Benefits.First().Type.Should().Be(BenefitTypeValue.ShopAccess);
        firstTierDto?.Benefits.First().Description.Should().Be("N1co shop disponible");
        firstTierDto?.Benefits.Last().Type.Should().Be(BenefitTypeValue.Rewards);
        firstTierDto?.Benefits.Last().Description.Should().Be("Premios de $0.50 a $15");
        
        var challenges = firstTierDto?.Challenges.ToList();
        var firstChallenge = challenges?.First();
        firstChallenge?.Stores.Should().BeEmpty();
        
        var expectedStatusValues = new List<LevelStatusValue>
        {
            LevelStatusValue.Completed, LevelStatusValue.InProgress, LevelStatusValue.Pending
        };
        result.LoyaltyProgram?.Tiers.Select(t => t.Status)
            .Should()
            .BeEquivalentTo(expectedStatusValues, options => options.WithStrictOrdering());
    }
    
    [Test]
    public async Task ShouldReturnLockedTiers()
    {
        // Arrange
        var profileMock = new LoyaltyProfileDto
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
                                    Type = ChallengeTypeValue.Referral,
                                    EffectValue = 200,
                                    EffectType = EffectTypeValue.Reward,
                                    EffectSubType = EffectSubTypeValue.Point,
                                }
                            ],
                            Benefits = [
                                new LoyaltyBenefitDto()
                                {
                                    Id = Guid.Empty.ToString(),
                                    Description = "N1co shop disponible",
                                    Type = BenefitTypeValue.ShopAccess,
                                }
                            ]
                        },
                        new LoyaltyTierDto
                        {
                            Id = Guid.Empty.ToString(),
                            Name = "Tier2",
                            IsCurrent = false,
                            IsLocked = true,
                            PointThreshold = 1000,
                            PointsToNextTier = 1000,
                            MotivationalMessage = "Proximamente",
                            Challenges =
                            [
                            ],
                        },
                        new LoyaltyTierDto
                        {
                            Id = Guid.Empty.ToString(),
                            Name = "Tier3",
                            IsCurrent = false,
                            IsLocked = true,
                            PointThreshold = 2000,
                            MotivationalMessage = "Proximamente",
                            Challenges =
                            [
                            ],
                        }
                    ]
                }
            ]
        };

        _loyaltyEngine = GetServiceMock<ILoyaltyEngine>();
        profileMock.Referral.IsActive = true;
        _loyaltyEngine.Setup(x => x.GetOrCreateProfile(It.IsAny<string>(), It.IsAny<LoyaltyCreateProfileInput>()))
            .ReturnsAsync(new ProfileCreationDto()
            {
                Success = true,
                Message = "Profile created",
                Profile = profileMock
            });

        var query = new GetProfileQuery();

        var result = await SendAsync(query);

        result.Should().NotBeNull();

        result.LoyaltyProgram?.Tiers.Count.Should().Be(3);
        result.LoyaltyProgram?.Tiers.First().Name.Should().Be("Tier1");
        result.LoyaltyProgram?.Tiers.First().Challenges.Count.Should().Be(1);
        result.LoyaltyProgram?.Tiers.First().Challenges.First().Name.Should().Be("Challenge1");
        result.LoyaltyProgram?.Tiers.First().Challenges.First().Type.Should().Be(ChallengeTypeValue.Referral);
        result.LoyaltyProgram?.Tiers.First().Challenges.First().Effect?.Amount.Should().Be(200);
        result.LoyaltyProgram?.Tiers.First().Challenges.First().Effect?.Type.Should().Be(EffectTypeValue.Reward);
        result.LoyaltyProgram?.Tiers.First().Challenges.First().Effect?.SubType.Should().Be(EffectSubTypeValue.Point);
        result.LoyaltyProgram?.Tiers.First().Benefits.Count.Should().Be(1);
        result.LoyaltyProgram?.Tiers.First().Benefits.First().Type.Should().Be(BenefitTypeValue.ShopAccess);
        result.LoyaltyProgram?.Tiers.First().Benefits.First().Description.Should().Be("N1co shop disponible");

        result.LoyaltyProgram?.Tiers.Count(t => t.IsLocked).Should().Be(2);
        result.LoyaltyProgram?.Tiers.Count(t => !t.IsLocked).Should().Be(1);
        
        result.LoyaltyProgram?.Tiers.First(t => t.IsLocked).MotivationalMessage.Should().Be("Proximamente");
    }

    [Test]
    public async Task ShouldReturnNullIfThereIsNoLoyaltyProgram()
    {
        var profileMock = new LoyaltyProfileDto
        {
            IntegrationId = "anyIdUser",
            PhoneNumber = "123456789",
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@email.com",
        };
        _loyaltyEngine = GetServiceMock<ILoyaltyEngine>();
        _loyaltyEngine.Setup(x => x.GetOrCreateProfile(It.IsAny<string>(), It.IsAny<LoyaltyCreateProfileInput>()))
            .ReturnsAsync(new ProfileCreationDto()
            {
                Success = true,
                Message = "Profile created",
                Profile = profileMock
            });
        var query = new GetProfileQuery();

        var result = await SendAsync(query);

        result.LoyaltyProgram.Should().BeNull();
    }

    [Test]
    public async Task ShouldReturnInactiveReferralCodeIfCurrentTierHasNoReferralChallenges()
    {
        // Arrange
        var profileMock = new LoyaltyProfileDto
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
                            PointsToNextTier = 998,
                            MotivationalMessage = "message",
                            Challenges = [
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
                                }
                            ],
                        }
                    ]
                }
            ]
        };

        _loyaltyEngine = GetServiceMock<ILoyaltyEngine>();
        profileMock.Referral.IsActive = true;
        _loyaltyEngine.Setup(x => x.GetProfile(It.IsAny<string>()))
            .ReturnsAsync(profileMock);

        var query = new GetProfileQuery();

        var result = await SendAsync(query);

        result.Referral.Should().NotBeNull();
        result.Referral?.Code.Should().Be(ReferralCode);
        result.Referral?.IsActive.Should().BeFalse();
        result.Referral?.RewardAmount.Should().Be(0);
    }

    [Test]
    public async Task ShouldReturnActiveReferralCodeIfCurrentReferralChallengesArePending()
    {
        // Arrange
        var profileMock = new LoyaltyProfileDto
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
                                    Type = ChallengeTypeValue.Referral,
                                    EffectValue = 200,
                                    EffectType = EffectTypeValue.Reward,
                                    EffectSubType = EffectSubTypeValue.Point,
                                }
                            ],
                        }
                    ]
                }
            ]
        };

        _loyaltyEngine = GetServiceMock<ILoyaltyEngine>();
        profileMock.Referral.IsActive = true;
        _loyaltyEngine.Setup(x => x.GetOrCreateProfile(It.IsAny<string>(), It.IsAny<LoyaltyCreateProfileInput>()))
            .ReturnsAsync(new ProfileCreationDto()
            {
                Success = true,
                Message = "Profile created",
                Profile = profileMock
            });

        var query = new GetProfileQuery();

        var result = await SendAsync(query);

        result.Referral.Should().NotBeNull();
        result.Referral?.Code.Should().Be(ReferralCode);
        result.Referral?.IsActive.Should().BeTrue();
        var loyaltyProgramDto = profileMock.LoyaltyPrograms.First();
        var loyaltyTierDto = loyaltyProgramDto.Tiers.First();
        var loyaltyProfileChallengeDto = loyaltyTierDto.Challenges.First();
        var effectValue = (int)(loyaltyProfileChallengeDto.EffectValue ?? 0);
        result.Referral?.RewardAmount.Should().Be(effectValue);
    }

    [Test]
    public async Task ShouldReturnInactiveReferralCodeIfCurrentReferralChallengesAreCompleted()
    {
        // Arrange
        var profileMock = new LoyaltyProfileDto
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
                            PointsToNextTier = 998,
                            MotivationalMessage = "message",
                            Challenges = [
                                new LoyaltyProfileChallengeDto
                                {
                                    Id = Guid.Empty.ToString(),
                                    Name = "Challenge1",
                                    Description = "Description",
                                    Target = 1,
                                    TargetProgress = 1,
                                    Type = ChallengeTypeValue.Referral,
                                    EffectValue = 200,
                                    EffectType = EffectTypeValue.Reward,
                                    EffectSubType = EffectSubTypeValue.Point,
                                }
                            ],
                        }
                    ]
                }
            ]
        };

        _loyaltyEngine = GetServiceMock<ILoyaltyEngine>();
        profileMock.Referral.IsActive = true;
        _loyaltyEngine.Setup(x => x.GetProfile(It.IsAny<string>()))
            .ReturnsAsync(profileMock);

        var query = new GetProfileQuery();

        var result = await SendAsync(query);

        result.Referral.Should().NotBeNull();
        result.Referral?.Code.Should().Be(ReferralCode);
        result.Referral?.IsActive.Should().BeFalse();
        result.Referral?.RewardAmount.Should().Be(0);
    }
}

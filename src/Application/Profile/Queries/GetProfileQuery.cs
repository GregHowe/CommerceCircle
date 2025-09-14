using Microsoft.Extensions.Configuration;
using N1coLoyalty.Application.Common.Constants;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Repositories;
using N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;
using N1coLoyalty.Application.Common.Models;
using N1coLoyalty.Application.Common.Services;
using N1coLoyalty.Application.Events.Commands;
using N1coLoyalty.Application.Profile.Services;
using N1coLoyalty.Domain.Entities;
using N1coLoyalty.Domain.Enums;
using LoyaltyProgramDto = N1coLoyalty.Application.Common.Models.LoyaltyProgramDto;
using LoyaltyProgramResponseDto = N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine.LoyaltyProgramDto;

namespace N1coLoyalty.Application.Profile.Queries;

public class GetProfileQuery : IRequest<ProfileDto>
{
    public class GetProfileQueryHandler(
        IConfiguration configuration,
        IUser currentUser,
        IUserRepository userRepository,
        ProfileService profileService,
        IApplicationDbContext context,
        UserWalletService userWalletService,
        ITransactionRepository transactionRepository
    )
        : IRequestHandler<GetProfileQuery, ProfileDto?>
    {
        public async Task<ProfileDto?> Handle(GetProfileQuery request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetOrCreateUserAsync(currentUser.ExternalId, currentUser.Phone);
            
            // Get or create profile
            var loyaltyProgramIntegrationId = configuration["LoyaltyCore:LoyaltyProgramIntegrationId"] ?? string.Empty;
            var profile = await profileService.GetOrCreateProfile(user, loyaltyProgramIntegrationId);
            if (profile is null) return null;
            
            // Generate profile DTO
            var loyaltyProgram = profile.LoyaltyPrograms.Find(lp => lp.IntegrationId == loyaltyProgramIntegrationId);
            var loyaltyProgramDto = GenerateLoyaltyProgramDto(loyaltyProgram);
            var referralDto = GetReferralDto(profile, loyaltyProgramDto);
            var profileDto = new ProfileDto
            {
                IntegrationId = profile.IntegrationId,
                PhoneNumber = profile.PhoneNumber,
                LoyaltyProgram = loyaltyProgramDto,
                Referral = referralDto,
                Balance = new BalanceDto()
                {
                    AvailableCoins = profile.Balance?.Credit ?? 0,
                    AccumulatedCoins = profile.Balance?.HistoricalCredit ?? 0
                },
                IsNew = false,
            };

            // Return profile if onboarding is completed
            if (user.OnboardingCompleted) return profileDto;

            // Complete onboarding: wallet creation
            var walletCreated = await context.UserWalletBalances.AnyAsync(wb =>
                wb.UserId == user.Id &&
                wb.Action == WalletActionValue.Create, cancellationToken: cancellationToken);
            if (!walletCreated)
            {
                var walletCreationResponse = await userWalletService.CreateWallet(user);
                if (walletCreationResponse is not null) walletCreated = true;
            }

            // Complete onboarding: onboarding reward
            var hasOnboardingReward = await context.Transactions.AnyAsync(t =>
                t.UserId == user.Id &&
                t.TransactionOrigin == TransactionOriginValue.Onboarding, cancellationToken);
            if (!hasOnboardingReward)
            {
                var rewardAmount = Convert.ToDecimal(configuration["Onboarding:Reward"]);
                var transactionModel = GetTransactionModel(user, rewardAmount);
                await context.BeginTransactionAsync();
                var transaction = await transactionRepository.CreateTransaction(transaction: transactionModel,
                    cancellationToken: cancellationToken);
                var creditWalletResponse = await userWalletService.Credit(transaction);
                if (creditWalletResponse is null)
                {
                    context.RollbackTransaction();
                }
                else
                {
                    await context.CommitTransactionAsync();
                    hasOnboardingReward = true;
                    profileDto.Balance.AvailableCoins = creditWalletResponse.Balance;
                    profileDto.Balance.AccumulatedCoins = creditWalletResponse.HistoricalCredit;
                }
            }

            // Update user onboarding status
            user.OnboardingCompleted = hasOnboardingReward && walletCreated;
            await context.SaveChangesAsync(cancellationToken: cancellationToken);
            profileDto.IsNew = user.OnboardingCompleted;

            return profileDto;
        }

        private static ReferralDto? GetReferralDto(LoyaltyProfileDto? profile, LoyaltyProgramDto? loyaltyProgram)
        {
            var currentTier = loyaltyProgram?.Tiers.FirstOrDefault(tier => tier.Status == LevelStatusValue.InProgress);

            var referralChallenge = currentTier?.Challenges.FirstOrDefault(challenge =>
                challenge.Type == ChallengeTypeValue.Referral);

            var isActive = referralChallenge != null && referralChallenge.Status != ChallengeStatusValue.Completed;
            var rewardAmount = isActive ? (int)(referralChallenge.Effect?.Amount ?? decimal.Zero) : 0;

            return profile?.Referral is not null
                ? new ReferralDto { Code = profile.Referral.Code, IsActive = isActive, RewardAmount = rewardAmount }
                : null;
        }

        private static LoyaltyProgramDto? GenerateLoyaltyProgramDto(LoyaltyProgramResponseDto? loyaltyProgram)
        {
            if (loyaltyProgram is null)
            {
                return null;
            }

            var loyaltyProgramDto = new LoyaltyProgramDto
            {
                IntegrationId = loyaltyProgram.IntegrationId,
                Name = loyaltyProgram.Name,
                Description = loyaltyProgram.Description,
                Tiers = loyaltyProgram.Tiers.Select(tier => new TierDto
                {
                    Id = new Guid(tier.Id),
                    Name = tier.Name,
                    MotivationalMessage = tier.MotivationalMessage,
                    PointThreshold = tier.PointThreshold,
                    PointsToNextTier = tier.PointsToNextTier,
                    Status = tier.IsCurrent ? LevelStatusValue.InProgress : LevelStatusValue.Pending,
                    IsLocked = tier.IsLocked,
                    Challenges = tier.Challenges.Select(c => new ChallengeDto
                    {
                        Id = new Guid(c.Id),
                        Name = c.Name,
                        Description = c.Description,
                        Type = c.Type,
                        Status = GetChallengeStatus(c),
                        Effect = new EffectDto
                        {
                            Type = c.EffectType, SubType = c.EffectSubType, Amount = c.EffectValue ?? decimal.Zero
                        },
                        Target = c.Target,
                        TargetProgress = c.TargetProgress,
                        Stores = c.Stores.Select(s => new StoreDto
                        {
                            Id = new Guid(s.Id), Name = s.Name, Description = s.Description, ImageUrl = s.ImageUrl,
                        }).ToList()
                    }).Where(c => c.Type != ChallengeTypeValue.UnlimitedExpense).ToList(),
                    Benefits = tier.Benefits.Select(b => new BenefitDto
                    {
                        Id = new Guid(b.Id), Description = b.Description, Type = b.Type
                    }).ToList(),
                }).ToList(),
            };

            var currentTier =
                loyaltyProgramDto.Tiers.FirstOrDefault(tier => tier.Status == LevelStatusValue.InProgress);
            if (currentTier == null) return loyaltyProgramDto;

            SetTierStatus(currentTier, loyaltyProgramDto.Tiers);

            return loyaltyProgramDto;
        }

        private static void SetTierStatus(TierDto currentTier, ICollection<TierDto> tiers)
        {
            foreach (var tier in tiers.Where(t => t.Status != LevelStatusValue.InProgress))
                tier.Status = currentTier.PointThreshold > tier.PointThreshold
                    ? LevelStatusValue.Completed
                    : LevelStatusValue.Pending;
        }

        private static ChallengeStatusValue GetChallengeStatus(LoyaltyProfileChallengeDto challenge)
        {
            if (challenge.TargetProgress == 0) return ChallengeStatusValue.Pending;
            return challenge.TargetProgress >= challenge.Target
                ? ChallengeStatusValue.Completed
                : ChallengeStatusValue.InProgress;
        }

        private static Transaction GetTransactionModel(User user, decimal rewardAmount)
        {
            return new Transaction()
            {
                Amount = rewardAmount,
                Name = TransactionName.OnboardingReward,
                Description = TransactionDescription.Onboarding,
                TransactionStatus = TransactionStatusValue.Redeemed,
                TransactionType = EffectTypeValue.Reward,
                TransactionSubType = EffectSubTypeValue.Point,
                TransactionOrigin = TransactionOriginValue.Onboarding,
                UserId = user.Id,
                User = user
            };
        }
    }
}

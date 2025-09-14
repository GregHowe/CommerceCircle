using N1coLoyalty.Application.Common.Constants;
using N1coLoyalty.Application.Common.Interfaces.Repositories;
using N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;
using N1coLoyalty.Domain.Enums.LoyaltyEngine;
using N1coLoyalty.Domain.Enums;
using N1coLoyalty.Domain.Events;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Services.CashBack;
using N1coLoyalty.Domain.Entities;
using N1coLoyalty.Domain.ValueObjects;

namespace N1coLoyalty.Application.Common.Services
{
    public class LoyaltyEventService(
        IApplicationDbContext dbContext,
        ITransactionRepository transactionRepository,
        ICashBackService cashBackService
    ) : ILoyaltyEventService
    {
        public async Task<Transaction?> ProcessEffect(LoyaltyEffectDto effect, User user)
        {
            var transaction = await ApplyEffect(user, effect);
            if (transaction is null) return null;
            await dbContext.SaveChangesAsync(CancellationToken.None);
            return transaction;
        }

        public async Task<List<Transaction>> ProcessEffects(List<LoyaltyEffectDto> effects, User user,
            LoyaltyEventDto? processedEvent = null, string? profileSessionId = null)

        {
            var transactions = new List<Transaction>();
            foreach (var effect in effects)
            {
                var transaction = await ApplyEffect(user, effect);
                if (transaction is null) continue;

                transactions.Add(transaction);
                transaction.ProfileSessionId = profileSessionId;

                if (processedEvent is null) continue;
                
                transaction.Event = GetEventModel(processedEvent);

                if (processedEvent.Attributes?.TryGetValue(CommonConstants.TransactionId, out var id) ?? false)
                {
                    transaction.IntegrationId = id.ToString();
                }

                if (effect.Challenge is not null && effect.Status == EffectStatus.Completed)
                    await AddChallengeCompletedDomainEvent(user, transaction, processedEvent);
                
                await dbContext.SaveChangesAsync(CancellationToken.None);
            }

            return transactions;
        }

        private async Task<Transaction?> ApplyEffect(User user, LoyaltyEffectDto effect)
        {
            Transaction? transaction = null;
            
            if (effect.Type is EffectTypeValue.Reward)
            {
                transaction = GetTransactionModelFromEffect(user: user, effectDto: effect);
                if (transaction is null) return null;
                
                var rewardActionType = GetRewardActionType(effect);

                switch (rewardActionType)
                {
                    //TODO: Add a different logic if the status is voided for cashback rewards
                    case RewardActionTypeValue.AddCash when effect.Status == EffectStatus.Completed:
                        var applyCashBackDto =
                            await cashBackService.ApplyCashBack(user.Phone ?? string.Empty, effect.Action.Amount,
                                TransactionDescription.Roulette, TransactionDescription.CashBackReward);
                        if (!applyCashBackDto.Success)
                        {
                            return null;
                        }

                        transaction = await transactionRepository.CreateTransaction(transaction: transaction,
                            cancellationToken: CancellationToken.None);
                        
                        var cashBackTransaction = applyCashBackDto.CashBackTransaction;
                        if (cashBackTransaction is not null) 
                            AddCashBackMetadata(transaction, cashBackTransaction);
                        
                        break;
                    case RewardActionTypeValue.AddPoints:
                        transaction = await transactionRepository.CreateTransaction(transaction: transaction,
                            cancellationToken: CancellationToken.None);
                        await AddUserWalletBalanceDomainEvent(user, effect, transaction);
                        break;
                    case RewardActionTypeValue.AddRetries when effect.Status == EffectStatus.Completed:
                        transaction = await transactionRepository.CreateTransaction(transaction: transaction,
                            cancellationToken: CancellationToken.None);
                        break;
                }
            }

            var notification = effect.Notification;
            if (notification is not null && effect.Status == EffectStatus.Completed) 
                await AddNotificationDomainEvent(user, notification, transaction);
            
            return transaction;
        }

        private static void AddCashBackMetadata(Transaction transaction, CashBackTransactionDto cashBackTransaction)
        {
            transaction.Metadata.TryAdd(TransactionMetadata.IssuingCashBackTransactionId,
                cashBackTransaction.OriginTransactionId ?? string.Empty);
            transaction.Metadata.TryAdd(TransactionMetadata.IssuingAppliedCashBackTransactionId,
                cashBackTransaction.Id ?? string.Empty);
            transaction.Metadata.TryAdd(TransactionMetadata.IssuingCashBackTransactionAmount,
                cashBackTransaction.Amount.ToString() ?? string.Empty);
        }

        private static RewardActionTypeValue GetRewardActionType(LoyaltyEffectDto effect)
        {
            return Enum.TryParse(effect.Action.Type, out RewardActionTypeValue actionType)
                ? actionType
                : RewardActionTypeValue.Unknown;
        }

        private static Transaction? GetTransactionModelFromEffect(User user, LoyaltyEffectDto effectDto)
        {
            Transaction? transaction = null;

            if (effectDto.Reward is null && effectDto.Challenge is null) return transaction;

            var ruleEffect = GetRuleEffectModel(effectDto);

            var subType = effectDto.Challenge?.EffectSubType ??
                          effectDto.Reward?.EffectSubType ?? EffectSubTypeValue.Unknown;

            var origin = GetTransactionOrigin(effectDto);

            (string name, string? description) = GetNameAndDescription(subType, effectDto);

            var transactionStatus = subType is EffectSubTypeValue.Retry
                ? TransactionStatusValue.Created
                : TransactionStatusValue.Redeemed;

            var effectTypeValue = EffectType.For(effectDto.Type.ToString()).Type;
            var transactionType = effectTypeValue switch
            {
                EffectTypeValue.Reward when effectDto.Status == EffectStatus.Completed => EffectTypeValue.Reward,
                EffectTypeValue.Reward when effectDto.Status == EffectStatus.Voided => EffectTypeValue.Revert,
                _ => EffectTypeValue.Unknown
            };
            var amount = effectDto.Action.Amount;

            transaction = new Transaction()
            {
                Name = name,
                Description = description,
                RuleEffect = ruleEffect,
                User = user,
                UserId = user.Id,
                TransactionStatus = transactionStatus,
                TransactionType = transactionType,
                TransactionSubType = subType,
                Amount = amount,
                TransactionOrigin = origin
            };

            transaction.Metadata.TryAdd(TransactionMetadata.CampaignId, effectDto.CampaignId);

            return transaction;
        }

        private static RuleEffect GetRuleEffectModel(LoyaltyEffectDto effectDto) =>
            new()
            {
                Id = effectDto.Id,
                Name = effectDto.Name,
                Type = effectDto.Type.ToString(),
                Status = effectDto.Status,
                CampaignId = effectDto.CampaignId,
                Action =
                    new LoyaltyEffectAction
                    {
                        Type = effectDto.Action.Type,
                        Amount = effectDto.Action.Amount,
                        Metadata = effectDto.Action.Metadata
                    },
                Challenge = effectDto.Challenge != null
                    ? new Challenge()
                    {
                        Id = effectDto.Challenge.Id,
                        Name = effectDto.Challenge.Name,
                        Description = effectDto.Challenge.Description,
                        EffectSubType = effectDto.Challenge.EffectSubType.ToString(),
                        Type = effectDto.Challenge.Type.ToString(),
                        EffectType = effectDto.Challenge.EffectType.ToString(),
                        EffectValue = effectDto.Challenge.EffectValue,
                        Target = effectDto.Challenge.Target,
                    }
                    : null,
                Reward = effectDto.Reward != null
                    ? new Reward
                    {
                        Id = effectDto.Reward.Id,
                        Name = effectDto.Reward.Name,
                        IntegrationId = effectDto.Reward.IntegrationId,
                    }
                    : null
            };

        private static Event GetEventModel(LoyaltyEventDto processedEvent)
        {
            var eventType = processedEvent.EventType;
            var eventAttributesAsStr = 
                processedEvent.Attributes?.ToDictionary(k => k.Key, v => v.Value.ToString() ?? string.Empty);

            return new Event
            {
                EventType = eventType,
                Attributes = eventAttributesAsStr
            };
        }

        private async Task AddUserWalletBalanceDomainEvent(User user, LoyaltyEffectDto effect, Transaction transaction)
        {
            user.AddDomainEvent(new UserWalletBalanceEvent
            {
                UserId = user.Id,
                Action = effect.Status == EffectStatus.Completed ? WalletActionValue.Credit : WalletActionValue.CreditVoid,
                Amount = effect.Action.Amount,
                Reason = transaction.Name,
                Reference = effect.Action.Metadata.TryGetValue(TransactionMetadata.TransactionId, out var transactionId)
                    ? transactionId
                    : string.Empty,
                TransactionId = transaction.Id
            });
            await dbContext.SaveChangesAsync(CancellationToken.None);
        }

        private async Task AddNotificationDomainEvent(User user, LoyaltyNotificationDto notification,
            Transaction? transaction)
        {
            user.AddDomainEvent(new NotificationEvent
            {
                SubscriberExternalId = user.ExternalUserId,
                Type = notification.Type,
                Title = notification.Title,
                Text = notification.Message,
                FormattedText = notification.FormattedMessage,
                ObjectId = transaction?.Id.ToString() ?? string.Empty,
            });
            await dbContext.SaveChangesAsync(CancellationToken.None);
        }

        private async Task AddChallengeCompletedDomainEvent(User user, Transaction transaction,
            LoyaltyEventDto processedEvent)
        {
            user.AddDomainEvent(new ChallengeCompletedEvent
            {
                User = user,
                Transaction = transaction,
                EventType = processedEvent.EventType,
                Attributes = processedEvent.Attributes,
            });

            await dbContext.SaveChangesAsync(CancellationToken.None);
        }

        private static (string, string?) GetNameAndDescription(EffectSubTypeValue subType, LoyaltyEffectDto effectDto)
        {
            (string name, string? description) = effectDto.Reward is not null
                ? subType switch
                {
                    EffectSubTypeValue.Cash => (TransactionName.CashbackReward, TransactionDescription.Roulette),
                    EffectSubTypeValue.Point => (TransactionName.PointsReward, TransactionDescription.Roulette),
                    EffectSubTypeValue.Compensation => (TransactionName.CompensationReward, TransactionDescription.Roulette),
                    EffectSubTypeValue.Retry => (TransactionName.RetryReward, TransactionDescription.Roulette),
                    EffectSubTypeValue.Unknown => ("N/A", "N/A"),
                    _ => throw new InvalidOperationException($"Unknown transaction sub type: {subType}")
                }
                : GetNameAndDescriptionForChallenge(effectDto);

            //TODO: Add more transaction names and descriptions for other voided transactions
            if (effectDto.Status == EffectStatus.Voided)
                name = TransactionName.Revert;

            return (name, description);
        }

        private static (string, string) GetNameAndDescriptionForChallenge(LoyaltyEffectDto effectDto)
        {
            if (effectDto.Challenge is null) return (string.Empty, string.Empty);

            return effectDto.Challenge.Type == ChallengeTypeValue.UnlimitedExpense
                ? (effectDto.Challenge.Name, TransactionDescription.UnlimitedChallenge)
                : (effectDto.Challenge.Name, TransactionDescription.Challenge);
        }

        private static TransactionOriginValue GetTransactionOrigin(LoyaltyEffectDto effectDto)
        {
            if (effectDto.Reward is not null && effectDto.Challenge is null)
            {
                return TransactionOriginValue.Game;
            }

            if (effectDto.Challenge is not null && effectDto.Reward is null)
            {
                return TransactionOriginValue.Challenge;
            }

            return TransactionOriginValue.Unknown;
        }
    }
}

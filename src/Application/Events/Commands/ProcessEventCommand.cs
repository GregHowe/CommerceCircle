using FluentValidation.Results;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using N1coLoyalty.Application.Common.Constants;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Repositories;
using N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;
using N1coLoyalty.Application.Common.Models;
using N1coLoyalty.Application.Common.Services;
using N1coLoyalty.Domain.Entities;
using N1coLoyalty.Domain.Enums;
using N1coLoyalty.Domain.ValueObjects;
using ValidationException = N1coLoyalty.Application.Common.Exceptions.ValidationException;

namespace N1coLoyalty.Application.Events.Commands;

public class ProcessEventCommand : IRequest<CommonServiceResponse<ProcessEventVm>>
{
    public EventTypeValue EventType { get; set; }

    public bool? IsExtraAttempt { get; set; } = false;

    public class ProcessEventCommandHandler(
        UserWalletService walletsService,
        IUser currentUser,
        ILoyaltyEngine loyaltyEngine,
        IUserRepository userRepository,
        ITransactionRepository transactionRepository,
        ILogger<ProcessEventCommandHandler> logger,
        IConfiguration configuration,
        IApplicationDbContext context,
        ILoyaltyEventService loyaltyEventService)
        : IRequestHandler<ProcessEventCommand, CommonServiceResponse<ProcessEventVm>>
    {
        public async Task<CommonServiceResponse<ProcessEventVm>> Handle(ProcessEventCommand request,
            CancellationToken cancellationToken)
        {
            var user = await userRepository.GetOrCreateUserAsync(currentUser.ExternalId, currentUser.Phone);

            var campaignIntegrationId =
                configuration[$"LoyaltyCore:CampaignIntegrationIds:{request.EventType.ToString()}"] ?? string.Empty;
            var campaign = await loyaltyEngine.GetCampaign(campaignIntegrationId);

            var beforeBalance = await walletsService.GetBalance(user);
            var unredeemedTransaction =
                await transactionRepository.GetOldestUnredeemedRetryTransaction(user.Id, cancellationToken);

            var isExtraAttempt = request.IsExtraAttempt ?? false;
            var eventCost = isExtraAttempt ? campaign!.ExtraAttemptCost : campaign!.EventCost;
            var isFreeEvent = !isExtraAttempt && unredeemedTransaction is not null;
            
            Transaction? debitTransaction = null;

            if (!isFreeEvent)
            {
                (CommonServiceResponse<ProcessEventVm>? processDebitEventCostResponse, Transaction? debitEventTransaction) =
                    await TryProcessDebitEventCost(user, beforeBalance, eventCost, isExtraAttempt, cancellationToken);
                if (processDebitEventCostResponse is not null && !processDebitEventCostResponse.Success)
                    return processDebitEventCostResponse;
                debitTransaction = debitEventTransaction;
            }

            var loyaltyProgramIntegrationId =
                configuration["LoyaltyCore:LoyaltyProgramIntegrationId"] ?? string.Empty;
            var processEventResponse = await loyaltyEngine.GetRewardByProbability(new RewardByProbabilityInputDto
            {
                LoyaltyProgramIntegrationId = loyaltyProgramIntegrationId,
                CampaignIntegrationId = campaignIntegrationId,
                LoyaltyProfile = new LoyaltyProfileDto
                {
                    IntegrationId = user.ExternalUserId,
                    FirstName = user.Name,
                    PhoneNumber = user.Phone,
                }
            });

            if (processEventResponse is not { Success: true, Effect: not null })
            {
                return isFreeEvent
                    ? GenerateFailedResponse(processEventResponse.Code, processEventResponse.Message)
                    : await WalletRefund(user, debitTransaction, processEventResponse.Code,
                        processEventResponse.Message, cancellationToken);
            }

            var effect = processEventResponse.Effect;
            var transaction = await loyaltyEventService.ProcessEffect(effect, user);

            if (transaction is null)
            {
                const string code = "GENERAL";
                const string message = "Error general: No se pudo acreditar el premio";
                return isFreeEvent
                    ? GenerateFailedResponse(code, message)
                    : await WalletRefund(user, debitTransaction, code, message, cancellationToken, effect.Id);
            }

            if (isFreeEvent) await RedeemTransaction(transaction, unredeemedTransaction, cancellationToken);
            return await GenerateSuccessResponse(user, isFreeEvent, eventCost, beforeBalance, effect);
        }

        private async Task RedeemTransaction(Transaction transaction, Transaction? unredeemedTransaction,
            CancellationToken cancellationToken)
        {
            if (unredeemedTransaction is not null)
            {
                var redeemedTransaction = await transactionRepository.UpdateTransactionStatus(
                    transaction: unredeemedTransaction, TransactionStatusValue.Redeemed,
                    cancellationToken: cancellationToken);

                // Add metadata to the new transaction
                transaction.Metadata.TryAdd(TransactionMetadata.RedeemedTransactionId, redeemedTransaction.Id.ToString());
                await transactionRepository.UpdateTransaction(transaction, cancellationToken);
            }
        }

        private async Task<CommonServiceResponse<ProcessEventVm>> WalletRefund(User user, Transaction? debitTransaction, string code,
            string message, CancellationToken cancellationToken, string? effectId = null)
        {
            if (debitTransaction is null)
                return GenerateFailedResponse(code, message);
            var refundTransaction = GetRefundTransactionModel(user, debitTransaction, effectId);

            await context.BeginTransactionAsync();
            await transactionRepository.CreateTransaction(refundTransaction, cancellationToken);
            var walletVoidResponse = await walletsService.Void(debitTransaction, refundTransaction);

            if (walletVoidResponse is not null)
            {
                await context.CommitTransactionAsync();
                logger.LogError(
                    "Process Event Error - Refund Success: Event Cost {EventCost} | UserId {UserId} | UserPhone {UserPhone}, ErrorCode {ErrorCode}, ErrorMessage {ErrorMessage}",
                    debitTransaction.Amount, user.ExternalUserId, user.Phone, code, message);
                return new CommonServiceResponse<ProcessEventVm>
                {
                    Success = false, Code = code, Message = message, Data = null
                };
            }

            context.RollbackTransaction();
            logger.LogError(
                "Process Event Error - Refund Error: Event Cost {EventCost} | UserId {UserId} | UserPhone {UserPhone} | ErrorCode {ErrorCode} | ErrorMessage {ErrorMessage}",
                debitTransaction.Amount, user.ExternalUserId, user.Phone, code, message);

            return new CommonServiceResponse<ProcessEventVm>
            {
                Success = false, Code = "WALLET_SERVICE_ERROR", Message = "Error general: No se pudo acreditar el costo de la ruleta", Data = null
            };
        }

        private static Transaction GetDebitTransactionModel(User user, decimal amount, bool isExtraAttempt = false)
        {
            return new Transaction()
            {
                Amount = amount,
                Name = isExtraAttempt ? TransactionName.RouletteExtraSpinDebit : TransactionName.RouletteSpinDebit,
                Description = TransactionDescription.Roulette,
                TransactionStatus = TransactionStatusValue.Redeemed,
                TransactionType = EffectTypeValue.Debit,
                TransactionSubType = EffectSubTypeValue.Point,
                TransactionOrigin = TransactionOriginValue.Game,
                UserId = user.Id,
                User = user
            };
        }

        private static Transaction GetRefundTransactionModel(User user, Transaction debitTransaction, string? effectId)
        {
            return new Transaction()
            {
                Amount = debitTransaction.Amount,
                Name = TransactionName.Refund,
                Description = TransactionDescription.Roulette,
                TransactionStatus = TransactionStatusValue.Redeemed,
                TransactionType = EffectTypeValue.Refund,
                TransactionSubType = EffectSubTypeValue.Point,
                TransactionOrigin = TransactionOriginValue.Game,
                UserId = user.Id,
                User = user,
                Metadata = new Dictionary<string, string>()
                {
                    {TransactionMetadata.VoidedTransactionId, debitTransaction.Id.ToString()},
                    {TransactionMetadata.EffectId, effectId ?? string.Empty}
                }
            };
        }

        private async Task<(CommonServiceResponse<ProcessEventVm>?, Transaction?)> TryProcessDebitEventCost(User user,
            UserWalletResponse? beforeBalance, decimal eventCost, bool isExtraAttempt, CancellationToken cancellationToken)
        {
            if (beforeBalance?.Balance < eventCost)
                throw new ValidationException(new List<ValidationFailure>()
                {
                    new("Balance", "Balance insuficiente para procesar el evento")
                });

            var transactionModel = GetDebitTransactionModel(user, eventCost, isExtraAttempt);
            await context.BeginTransactionAsync();
            var transaction = await transactionRepository.CreateTransaction(transaction: transactionModel, cancellationToken: cancellationToken);
            var walletDebitResponse = await walletsService.Debit(transaction);
            if (walletDebitResponse.Data is null)
            {
                context.RollbackTransaction();
                return (new CommonServiceResponse<ProcessEventVm>
                { Success = walletDebitResponse.Success, Code = walletDebitResponse.Code, Message = walletDebitResponse.Message, Data = null  } , null);
            }

            await context.CommitTransactionAsync();
            return (new CommonServiceResponse<ProcessEventVm>
            {
                Success = walletDebitResponse.Success,
                Code = walletDebitResponse.Code,
                Message = walletDebitResponse.Message,
                Data = new ProcessEventVm()
                {
                    Balance = new EffectBalanceDto()
                    {
                        Before = beforeBalance?.Balance, Current = walletDebitResponse.Data.Balance
                    }
                }
            }, transaction);
        }

        private static CommonServiceResponse<ProcessEventVm> GenerateFailedResponse(string code, string message)
        {
            return new CommonServiceResponse<ProcessEventVm>
            {
                Success = false,
                Code = code,
                Message = message,
                Data = null
            };
        }

        private async Task<CommonServiceResponse<ProcessEventVm>> GenerateSuccessResponse(User user, bool isFreeEvent,
            decimal eventCost, UserWalletResponse? beforeBalance, LoyaltyEffectDto effect)
        {
            var afterBalance = await walletsService.GetBalance(user);

            return new CommonServiceResponse<ProcessEventVm>
            {
                Success = true,
                Code = "OK",
                Message = "Ruleta girada exitosamente",
                Data = new ProcessEventVm
                {
                    EventCost = isFreeEvent ? 0 : eventCost,
                    Balance =
                        new EffectBalanceDto
                        {
                            Before = beforeBalance?.Balance,
                            Current = afterBalance?.Balance
                        },
                    Effect = new EffectDto
                    {
                        Type = EffectType.For(effect.Type.ToString()).Type,
                        SubType = TransactionSubType.For(effect.Reward?.IntegrationId ?? string.Empty)
                            .SubType,
                        Amount = effect.Action.Amount,
                    }
                }
            };
        }
    }
}

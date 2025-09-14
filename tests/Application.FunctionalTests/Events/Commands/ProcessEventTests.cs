using N1coLoyalty.Application.Common.Constants;
using N1coLoyalty.Application.Common.Exceptions;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Services.CashBack;
using N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;
using N1coLoyalty.Application.Common.Models;
using N1coLoyalty.Application.Events.Commands;
using N1coLoyalty.Application.FunctionalTests.Helpers;
using N1coLoyalty.Domain.Entities;
using N1coLoyalty.Domain.Enums;

namespace N1coLoyalty.Application.FunctionalTests.Events.Commands;

using static Testing;
using static TermsConditionsHelpers;
using static CampaignHelpers;

public class ProcessEventTests : BaseTestFixture
{
    private Mock<ILoyaltyEngine> _loyaltyEngine = new();
    private Mock<IWalletsService> _walletsService = new();
    private Mock<ICashBackService> _cashBackService = new();
    private TermsConditionsInfo _termsConditionsInfo;

    // TODO: (Richard) Add tests for extra attempts

    [SetUp]
    public async Task SetUp()
    {
        _termsConditionsInfo = await CreateTermsConditions();
    }

    [Test]
    public async Task Should_Process_Event()
    {
        // Arrange

        #region LoyaltyEngine service Arrange

        var campaign = await CreateCampaignMock();
        const int eventAttempts = 10;

        campaign.Rewards = campaign.Rewards.Where(r => r.IntegrationId != "RETRY").ToList();

        _loyaltyEngine = GetServiceMock<ILoyaltyEngine>();
        _loyaltyEngine.Setup(x => x.GetCampaign(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>()))
            .ReturnsAsync(campaign);

        #endregion

        var processEventResponse = new RewardByProbabilityResponseDto()
        {
            Success = true,
            Code = "OK",
            Effect = new LoyaltyEffectDto()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Has ganado coins",
                Type = EffectTypeValue.Reward,
                Status = EffectStatus.Completed,
                Action = new LoyaltyEffectActionDto
                {
                    Type = "AddPoints",
                    Amount = 1000,
                    Metadata = new Dictionary<string, string> { { "transactionId", "t-id" } }
                },
                CampaignId = "campaign-id",
                Reward = new LoyaltyRewardDto
                {
                    Id = Guid.NewGuid(),
                    IntegrationId = "POINT",
                    Name = "Has ganado coins",
                    EffectSubType = EffectSubTypeValue.Point
                }
            },
            Message = "Evento procesado exitosamente"
        };
        processEventResponse.Effect.Action.Metadata.TryAdd("transactionId", "C7BECA1F-C752-4AFF-41F9-08DCC2242E06");

        _loyaltyEngine.Setup(x => x.GetRewardByProbability(It.IsAny<RewardByProbabilityInputDto>()))
            .ReturnsAsync(processEventResponse);

        #region wallert service Arrange

        _walletsService = GetServiceMock<IWalletsService>();

        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto
            {
                Credit = 500,
                Debit = 0,
                TransactionId = "f181aa94-838a-418f-83e5-ed7f4bc1253a"
            });

        _walletsService.Setup(x => x.Debit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto
            {
                Credit = 250,
                Debit = 0,
                TransactionId = "f181aa94-838a-418f-83e5-ed7f4bc1253b"
            });

        _walletsService.Setup(x => x.Credit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto
            {
                Credit = 100,
                Debit = 0,
                TransactionId = "f181aa94-838a-418f-83e5-ed7f4bc1253c"
            });

        #endregion

        // Act
        var random = new Random();
        var randomNumbers = Enumerable.Range(0, eventAttempts).Select(_ => random.Next(0, 100)).ToList();

        var responses = new List<CommonServiceResponse<ProcessEventVm>>();
        foreach (var unused in randomNumbers)
        {
            var commonServiceResponse =
                await SendAsync(new ProcessEventCommand { EventType = EventTypeValue.PlayGame, IsExtraAttempt = false });

            commonServiceResponse.Success.Should().BeTrue();
            commonServiceResponse.Message.Should().NotBeNullOrEmpty();
            commonServiceResponse.Code.Should().Be("OK");

            var processEventVm = commonServiceResponse.Data;
            processEventVm?.EventCost.Should().Be(250);
            processEventVm?.Balance.Before.Should().Be(500);
            responses.Add(commonServiceResponse);
        }

        // Assert
        responses.Count.Should().Be(eventAttempts);
        responses.TrueForAll(x => x.Success).Should().BeTrue();

        // check transactions for rewards
        var transactions = await ToListAsync<Transaction>(x => x.TransactionType == EffectTypeValue.Reward);
        transactions.Should().HaveCount(eventAttempts);
        // check transactions for debits
        var debitTransactions = await ToListAsync<Transaction>(x => x.TransactionType == EffectTypeValue.Debit);
        debitTransactions.Should().HaveCount(eventAttempts);

        // check transactions metadata
        foreach (var transaction in transactions)
        {
            transaction.Metadata.ContainsKey(TransactionMetadata.CampaignId).Should().BeTrue();
            transaction.RuleEffect.Should().NotBeNull();
            transaction.RuleEffect?.Action.Should().NotBeNull();

            if (transaction.TransactionSubType == EffectSubTypeValue.Point)
                transaction.Name.Should().Be(TransactionName.PointsReward);
            if (transaction.TransactionSubType == EffectSubTypeValue.Compensation)
                transaction.Name.Should().Be(TransactionName.CompensationReward);
            if (transaction.TransactionSubType == EffectSubTypeValue.Retry)
                transaction.Name.Should().Be(TransactionName.RetryReward);
            if (transaction.TransactionSubType == EffectSubTypeValue.Cash)
                transaction.Name.Should().Be(TransactionName.CashbackReward);

            transaction.Description.Should().Be(TransactionDescription.Roulette);
        }

        //Check wallet balance
        var walletBalanceList = await ToListAsync<UserWalletBalance>(x => true);
        var effectTypeCount = responses.Count(r =>
            r.Data?.Effect?.SubType is EffectSubTypeValue.Point or EffectSubTypeValue.Compensation);
        walletBalanceList.Should().HaveCount(effectTypeCount + eventAttempts);
        foreach (var userWalletBalance in walletBalanceList)
        {
            userWalletBalance.Reference.Should().NotBeNullOrEmpty();
            userWalletBalance.UserId.Should().NotBeEmpty();
            userWalletBalance.IsDeleted.Should().BeFalse();

            if (userWalletBalance.Action == WalletActionValue.Credit)
                userWalletBalance.TransactionId.Should().NotBeNull();
        }

        var walletBalanceGroup = walletBalanceList.GroupBy(r => r.Action);
        var walletBalanceGroupCount =
            walletBalanceGroup.Select(g => new { Action = g.Key, Count = g.Count() }).ToList();

        walletBalanceGroupCount.First(x => x.Action == WalletActionValue.Credit).Count.Should().Be(effectTypeCount);
        walletBalanceGroupCount.First(x => x.Action == WalletActionValue.Debit).Count.Should().Be(eventAttempts);
    }

    [Test]
    public async Task Should_Fail_Process_Event()
    {
        // Arrange

        #region LoyaltyEngine service Arrange

        var campaign = await CreateCampaignMock();

        campaign.Rewards = campaign.Rewards.Where(r => r.IntegrationId != "RETRY").ToList();

        _loyaltyEngine = GetServiceMock<ILoyaltyEngine>();
        _loyaltyEngine.Setup(x => x.GetCampaign(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>()))
            .ReturnsAsync(campaign);

        var processEventResponse = new RewardByProbabilityResponseDto()
        {
            Success = true,
            Code = "OK",
            Effect = new LoyaltyEffectDto()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Has ganado coins",
                Type = EffectTypeValue.Unknown,
                Status = EffectStatus.Completed,
                Action = new LoyaltyEffectActionDto
                {
                    Type = "AddPoints",
                    Amount = 1000,
                    Metadata = new Dictionary<string, string> { { "transactionId", "t-id" } }
                },
                CampaignId = "campaign-id",
                Reward = new LoyaltyRewardDto
                {
                    Id = Guid.NewGuid(),
                    IntegrationId = "UNKNOWN",
                    Name = "Has ganado coins",
                    EffectSubType = EffectSubTypeValue.Unknown
                }
            },
            Message = "Evento procesado exitosamente"
        };

        _loyaltyEngine.Setup(x => x.GetRewardByProbability(It.IsAny<RewardByProbabilityInputDto>()))
            .ReturnsAsync(processEventResponse);

        #endregion

        #region wallet service Arrange

        _walletsService = GetServiceMock<IWalletsService>();

        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto
            {
                Credit = 500,
                Debit = 0,
                TransactionId = "f181aa94-838a-418f-83e5-ed7f4bc1253a"
            });

        _walletsService.Setup(x => x.Debit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto
            {
                Credit = 250,
                Debit = 0,
                TransactionId = "f181aa94-838a-418f-83e5-ed7f4bc1253b"
            });

        _walletsService.Setup(x => x.Credit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto
            {
                Credit = 100,
                Debit = 0,
                TransactionId = "f181aa94-838a-418f-83e5-ed7f4bc1253c"
            });

        _walletsService.Setup(x => x.Void(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto
            {
                Credit = 250,
                Debit = 0,
                TransactionId = "f181aa94-838a-418f-83e5-ed7f4bc1253d"
            });

        #endregion

        // Act

        var commonServiceResponse = await SendAsync(new ProcessEventCommand { EventType = EventTypeValue.PlayGame, IsExtraAttempt = false });

        commonServiceResponse.Success.Should().BeFalse();
        commonServiceResponse.Message.Should().NotBeNullOrEmpty();
        commonServiceResponse.Message.Should().Be("Error general: No se pudo acreditar el premio");
        commonServiceResponse.Code.Should().Be("GENERAL");

        var processEventVm = commonServiceResponse.Data;
        processEventVm?.EventCost.Should().Be(250);
        processEventVm?.Balance.Before.Should().Be(500);
        processEventVm?.Effect.Should().BeNull();
    }

    [Test]
    public async Task Should_Not_Process_Event_When_Balance_Is_Not_Enough()
    {
        // Arrange
        _walletsService = GetServiceMock<IWalletsService>();

        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 0, Debit = 0 });

        // Act
        var command = new ProcessEventCommand { EventType = EventTypeValue.PlayGame, IsExtraAttempt = false };

        // Assert
        var exceptionAssertions = await FluentActions.Invoking(() =>
            SendAsync(command)).Should().ThrowAsync<ValidationException>();

        var errors = exceptionAssertions.Which.Errors;
        errors.Should().HaveCount(1);
        errors.Should().ContainKey("Balance");
        errors["Balance"].Should().Contain("Balance insuficiente para procesar el evento");
    }

    [Test]
    public async Task Should_Not_Exceed_Campaign_Event_Frequency_Limit()
    {
        // Arrange

        #region LoyaltyEngine service Arrange

        var campaign = await CreateCampaignMock();
        // update campaign event frequency limit
        campaign.UserEventFrequencyLimit = 1;

        _loyaltyEngine = GetServiceMock<ILoyaltyEngine>();
        _loyaltyEngine.Setup(x => x.GetCampaign(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>()))
            .ReturnsAsync(campaign);

        var processEventResponse = new RewardByProbabilityResponseDto()
        {
            Success = true,
            Code = "OK",
            Effect = new LoyaltyEffectDto()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Has acumulado co1ns",
                Type = EffectTypeValue.Reward,
                Status = EffectStatus.Completed,
                Action = new LoyaltyEffectActionDto
                {
                    Type = "AddPoints",
                    Amount = 1000,
                    Metadata = new Dictionary<string, string> { { "transactionId", "t-id" } }
                },
                CampaignId = "campaign-id",
                Reward = new LoyaltyRewardDto
                {
                    Id = Guid.NewGuid(),
                    IntegrationId = "POINT",
                    Name = "Has acumulado co1ns"
                }
            },
        };
        processEventResponse.Effect.Action.Metadata.TryAdd("transactionId", "C7BECA1F-C752-4AFF-41F9-08DCC2242E06");

        _loyaltyEngine.Setup(x => x.GetRewardByProbability(It.IsAny<RewardByProbabilityInputDto>()))
            .ReturnsAsync(processEventResponse);

        #endregion

        // Arrange
        _walletsService = GetServiceMock<IWalletsService>();

        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 5000, Debit = 0 });

        _walletsService.Setup(x => x.Debit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 250, Debit = 0 });

        _walletsService.Setup(x => x.Credit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 100, Debit = 0 });

        // Act
        var command = new ProcessEventCommand { EventType = EventTypeValue.PlayGame, IsExtraAttempt = false };

        // Trigger the first event
        await SendAsync(command);

        // Assert
        var exceptionAssertions = await FluentActions.Invoking(() =>
                SendAsync(command))
            .Should()
            .ThrowAsync<ValidationException>();

        var errors = exceptionAssertions.Which.Errors;
        errors.Should().HaveCount(1);
        errors.Should().ContainKey("");
        errors[""].Should().Contain("Has excedido el límite de intentos.");
    }

    [Test]
    public async Task Should_Not_Process_Event_When_Campaign_Not_Exists()
    {
        LoyaltyCampaignDto campaign = null!;
        _loyaltyEngine = GetServiceMock<ILoyaltyEngine>();
        _loyaltyEngine.Setup(x => x.GetCampaign(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>()))
            .ReturnsAsync(campaign);

        // Arrange
        _walletsService = GetServiceMock<IWalletsService>();

        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 0, Debit = 0 });

        // Act
        var command = new ProcessEventCommand { EventType = EventTypeValue.PlayGame, IsExtraAttempt = false };

        // Assert
        var exceptionAssertions = await FluentActions.Invoking(() =>
            SendAsync(command)).Should().ThrowAsync<ValidationException>();

        var errors = exceptionAssertions.Which.Errors;
        errors.Should().HaveCount(1);
        errors.Should().ContainKey("");
        errors[""].Should().Contain("No existe una campaña activa.");
    }

    [Test]
    public async Task Should_Fail_When_Wallet_Debit_Fails()
    {
        // Arrange
        _walletsService = GetServiceMock<IWalletsService>();

        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 5000, Debit = 0 });

        _walletsService.Setup(x => x.Debit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(() => null);

        // Act
        var command = new ProcessEventCommand { EventType = EventTypeValue.PlayGame, IsExtraAttempt = false };
        var response = await SendAsync(command);

        // Assert
        response.Success.Should().BeFalse();
        response.Message.Should().NotBeNullOrEmpty();
        response.Code.Should().Be("WALLET_SERVICE_DEBIT_ERROR");

        var walletBalanceList = await ToListAsync<UserWalletBalance>(x => true);
        walletBalanceList.Should().BeEmpty();
    }

    [Test]
    public async Task Should_Fail_When_Budget_Is_Not_Enough()
    {
        // Arrange

        #region LoyaltyEngine service Arrange

        var campaign = await CreateCampaignMock();

        // update campaign budget
        campaign.TotalBudget = 0;

        _loyaltyEngine = GetServiceMock<ILoyaltyEngine>();
        _loyaltyEngine.Setup(x => x.GetCampaign(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>()))
            .ReturnsAsync(campaign);

        var processEventResponse = new RewardByProbabilityResponseDto()
        {
            Success = false,
            Code = "BUDGET_EXCEEDED",
            Message = "Presupuesto excedido."
        };
        _loyaltyEngine.Setup(x => x.GetRewardByProbability(It.IsAny<RewardByProbabilityInputDto>()))
            .ReturnsAsync(processEventResponse);

        #endregion

        _walletsService = GetServiceMock<IWalletsService>();

        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 5000, Debit = 0 });

        _walletsService.Setup(x => x.Debit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto
            {
                Credit = 250,
                Debit = 0,
                TransactionId = "f181aa94-835d-418f-83e5-ed7f4bc1253d"
            });

        _walletsService.Setup(x => x.Credit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 100, Debit = 0 });

        _walletsService.Setup(x => x.Void(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto
            {
                Credit = 250,
                Debit = 0,
                TransactionId = "f181aa94-838a-418f-83e5-ed7f4bc1253d"
            });

        // Act
        var command = new ProcessEventCommand { EventType = EventTypeValue.PlayGame, IsExtraAttempt = false };
        var response = await SendAsync(command);

        // Assert
        response.Success.Should().BeFalse();
        response.Message.Should().NotBeNullOrEmpty();
        response.Code.Should().Be("BUDGET_EXCEEDED");

        var walletBalanceList = await ToListAsync<UserWalletBalance>(x => true);
        walletBalanceList.Should().HaveCount(2);

        walletBalanceList.Select(x => x.Action).Should().Contain([WalletActionValue.Debit, WalletActionValue.DebitVoid]);

        var transactions = await ToListAsync<Transaction>(x => true);
        transactions.Should().HaveCount(2);

        var debitTransaction = transactions.First(x => x.TransactionType == EffectTypeValue.Debit);
        var refundTransaction = transactions.First(x => x.TransactionType == EffectTypeValue.Refund);

        debitTransaction.TransactionStatus.Should().Be(TransactionStatusValue.Voided);
        debitTransaction.Metadata.ContainsKey(TransactionMetadata.VoidTransactionId).Should().BeTrue();
        debitTransaction.Metadata.ContainsValue(refundTransaction.Id.ToString()).Should().BeTrue();

        refundTransaction.TransactionStatus.Should().Be(TransactionStatusValue.Redeemed);
        refundTransaction.Metadata.ContainsKey(TransactionMetadata.VoidedTransactionId).Should().BeTrue();
        refundTransaction.Metadata.ContainsValue(debitTransaction.Id.ToString()).Should().BeTrue();
    }

    [Test]
    public async Task Should_Fail_When_Wallet_Refund_Fails()
    {
        // Arrange

        #region LoyaltyEngine service Arrange

        var campaign = await CreateCampaignMock();

        // update campaign budget
        campaign.TotalBudget = 0;

        _loyaltyEngine = GetServiceMock<ILoyaltyEngine>();
        _loyaltyEngine.Setup(x => x.GetCampaign(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>()))
            .ReturnsAsync(campaign);


        var processEventResponse = new RewardByProbabilityResponseDto()
        {
            Success = false,
            Code = "WALLET_SERVICE_ERROR",
            Message = "Error general: No se pudo acreditar el costo de la ruleta"
        };
        _loyaltyEngine.Setup(x => x.GetRewardByProbability(It.IsAny<RewardByProbabilityInputDto>()))
            .ReturnsAsync(processEventResponse);

        #endregion

        _walletsService = GetServiceMock<IWalletsService>();

        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 5000, Debit = 0 });

        _walletsService.Setup(x => x.Debit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 250, Debit = 0 });

        _walletsService.Setup(x => x.Void(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(() => null);

        // Act
        var command = new ProcessEventCommand { EventType = EventTypeValue.PlayGame, IsExtraAttempt = false };
        var response = await SendAsync(command);

        // Assert
        response.Success.Should().BeFalse();
        response.Message.Should().NotBeNullOrEmpty();
        response.Code.Should().Be("WALLET_SERVICE_ERROR");
        response.Message.Should().Be("Error general: No se pudo acreditar el costo de la ruleta");

        var walletBalanceList = await ToListAsync<UserWalletBalance>(x => true);
        walletBalanceList.Should().HaveCount(1);
        walletBalanceList[0].Action.Should().Be(WalletActionValue.Debit);
    }

    [Test]
    public async Task Should_Fail_When_Point_Effect_Action_Fails()
    {
        // Arrange

        #region LoyaltyEngine service Arrange

        var campaign = await CreateCampaignMock([
            new LoyaltyRewardDto
            {
                IntegrationId = "POINT",
                Name = "Co1ns para subir de nivel y canjear",
                EffectSubType = EffectSubTypeValue.Point
            }
        ]);
        _loyaltyEngine = GetServiceMock<ILoyaltyEngine>();
        _loyaltyEngine.Setup(x => x.GetCampaign(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>()))
            .ReturnsAsync(campaign);

        var processEventResponse = new RewardByProbabilityResponseDto()
        {
            Success = false,
            Code = "WALLET_SERVICE_ERROR",
            Message = "Error general: No se pudo acreditar el premio"
        };
        _loyaltyEngine.Setup(x => x.GetRewardByProbability(It.IsAny<RewardByProbabilityInputDto>()))
            .ReturnsAsync(processEventResponse);

        #endregion


        _walletsService = GetServiceMock<IWalletsService>();

        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 5000, Debit = 0 });

        _walletsService.Setup(x => x.Debit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 250, Debit = 0 });

        _walletsService.Setup(x => x.Credit(It.IsAny<string>(), 100))
            .ReturnsAsync(() => null);

        _walletsService.Setup(x => x.Void(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 250, Debit = 0, TransactionId = "f181aa94-838a-418f-83e5-ed7f4bc1253d" });

        // Act
        var command = new ProcessEventCommand { EventType = EventTypeValue.PlayGame, IsExtraAttempt = false };
        var response = await SendAsync(command);

        // Assert
        response.Success.Should().BeFalse();
        response.Message.Should().NotBeNullOrEmpty();
        response.Code.Should().Be("WALLET_SERVICE_ERROR");
        response.Message.Should().Be("Error general: No se pudo acreditar el premio");

        var walletBalanceList = await ToListAsync<UserWalletBalance>(x => true);
        walletBalanceList.Should().HaveCount(2);

        walletBalanceList.Select(x => x.Action).Should().Contain([WalletActionValue.DebitVoid, WalletActionValue.Debit]);

        var transactions = await ToListAsync<Transaction>(x => true);
        transactions.Should().HaveCount(2);

        var debitTransaction = transactions.First(x => x.TransactionType == EffectTypeValue.Debit);
        var refundTransaction = transactions.First(x => x.TransactionType == EffectTypeValue.Refund);

        debitTransaction.TransactionStatus.Should().Be(TransactionStatusValue.Voided);
        debitTransaction.Metadata.ContainsKey(TransactionMetadata.VoidTransactionId).Should().BeTrue();
        debitTransaction.Metadata.ContainsValue(refundTransaction.Id.ToString()).Should().BeTrue();

        refundTransaction.TransactionStatus.Should().Be(TransactionStatusValue.Redeemed);
        refundTransaction.Metadata.ContainsKey(TransactionMetadata.VoidedTransactionId).Should().BeTrue();
        refundTransaction.Metadata.ContainsValue(debitTransaction.Id.ToString()).Should().BeTrue();
    }

    [Test]
    public async Task Should_Save_Effect_Id_In_Refund_Transaction_Metadata_When_Process_Effect_Fails()
    {
        // Arrange

        #region LoyaltyEngine service Arrange

        var campaign = await CreateCampaignMock([
            new LoyaltyRewardDto
            {
                IntegrationId = "CASH",
                Name = "Efectivo en tu tarjeta n1co",
                EffectSubType = EffectSubTypeValue.Cash
            }
        ]);
        _loyaltyEngine = GetServiceMock<ILoyaltyEngine>();
        _loyaltyEngine.Setup(x => x.GetCampaign(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>()))
            .ReturnsAsync(campaign);

        var processEventResponse = new RewardByProbabilityResponseDto()
        {
            Success = true,
            Effect = new LoyaltyEffectDto()
            {
                Id = "effect-id",
                Name = "Cashback en tarjeta n1co",
                Type = EffectTypeValue.Reward,
                Status = EffectStatus.Completed,
                Action = new LoyaltyEffectActionDto
                {
                    Type = "AddCash",
                    Amount = 4,
                    Metadata = new Dictionary<string, string> { { "transactionId", "t-id" } }
                },
                CampaignId = "campaign-id",
                Reward = new LoyaltyRewardDto
                {
                    Id = Guid.NewGuid(),
                    IntegrationId = "CASH",
                    Name = "Cashback en tarjeta n1co",
                    EffectSubType = EffectSubTypeValue.Cash
                }
            },
        };
        processEventResponse.Effect.Action.Metadata.TryAdd("transactionId", "C7BECA1F-C752-4AFF-41F9-08DCC2242E06");

        _loyaltyEngine.Setup(x => x.GetRewardByProbability(It.IsAny<RewardByProbabilityInputDto>()))
            .ReturnsAsync(processEventResponse);

        #endregion


        #region WalletServiceMock Arrange

        // process an event with cost to get a reward
        _walletsService = GetServiceMock<IWalletsService>();

        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0 });

        _walletsService.Setup(x => x.Debit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 250, Debit = 0 });

        _walletsService.Setup(x => x.Void(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 250, Debit = 0, TransactionId = "f181aa94-838a-418f-83e5-ed7f4bc1253d" });

        #endregion

        _cashBackService = GetServiceMock<ICashBackService>();
        _cashBackService.Setup(x => x.ApplyCashBack(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(() => new ApplyCashBackDto()
            {
                Success = false,
                Code = "CASHBACK_ERROR",
                Message = "Error general: No se pudo acreditar el premio"
            });

        // try to process a free event redeeming the reward
        var processEventCommandResponse =
            await SendAsync(new ProcessEventCommand { EventType = EventTypeValue.PlayGame, IsExtraAttempt = false });

        // Assert
        processEventCommandResponse.Success.Should().BeFalse();
        processEventCommandResponse.Message.Should().NotBeNullOrEmpty();
        processEventCommandResponse.Code.Should().Be("GENERAL");
        processEventCommandResponse.Message.Should().Be("Error general: No se pudo acreditar el premio");

        var walletBalanceList = await ToListAsync<UserWalletBalance>(x => true);
        walletBalanceList.Should().HaveCount(2);

        walletBalanceList.Select(x => x.Action).Should().Contain([WalletActionValue.DebitVoid, WalletActionValue.Debit]);

        var transactions = await ToListAsync<Transaction>(x => true);
        transactions.Should().HaveCount(2);

        var debitTransaction = transactions.First(x => x.TransactionType == EffectTypeValue.Debit);
        var refundTransaction = transactions.First(x => x.TransactionType == EffectTypeValue.Refund);

        debitTransaction.TransactionStatus.Should().Be(TransactionStatusValue.Voided);
        debitTransaction.Metadata.ContainsKey(TransactionMetadata.VoidTransactionId).Should().BeTrue();
        debitTransaction.Metadata.ContainsValue(refundTransaction.Id.ToString()).Should().BeTrue();

        refundTransaction.TransactionStatus.Should().Be(TransactionStatusValue.Redeemed);
        refundTransaction.Metadata.ContainsKey(TransactionMetadata.VoidedTransactionId).Should().BeTrue();
        refundTransaction.Metadata.ContainsValue(debitTransaction.Id.ToString()).Should().BeTrue();
        refundTransaction.Metadata.ContainsKey(TransactionMetadata.EffectId).Should().BeTrue();
        refundTransaction.Metadata.ContainsValue("effect-id").Should().BeTrue();
    }

    [Test]
    public async Task Should_Fail_When_DoesNot_Exist_TermsAndConditions()
    {
        var termsConditionsAcceptedFromDb = await FirstAsync<TermsConditionsAcceptance>(r => true);
        await RemoveAsync(termsConditionsAcceptedFromDb);
        await RemoveAsync(_termsConditionsInfo);

        _walletsService = GetServiceMock<IWalletsService>();

        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 5000, Debit = 0 });

        _walletsService.Setup(x => x.Debit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(() => null);

        // Act
        var command = new ProcessEventCommand { EventType = EventTypeValue.PlayGame, IsExtraAttempt = false };
        var response = await SendAsync(command);

        // Assert
        response.Success.Should().BeFalse();
        response.Message.Should().NotBeNullOrEmpty();
        response.Code.Should().Be("TERMS_CONDITIONS_DOESNT_EXIST");

        var walletBalanceList = await ToListAsync<UserWalletBalance>(x => true);
        walletBalanceList.Should().BeEmpty();
    }

    [Test]
    public async Task Should_Fail_When_TermsAndConditions_Not_Accepted()
    {
        var termsConditionsAcceptedFromDb = await FirstAsync<TermsConditionsAcceptance>(r => true);
        termsConditionsAcceptedFromDb.IsAccepted = false;
        await UpdateAsync(termsConditionsAcceptedFromDb);

        _walletsService = GetServiceMock<IWalletsService>();

        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 5000, Debit = 0 });

        _walletsService.Setup(x => x.Debit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(() => null);

        // Act
        var command = new ProcessEventCommand { EventType = EventTypeValue.PlayGame, IsExtraAttempt = false };
        var response = await SendAsync(command);

        // Assert
        response.Success.Should().BeFalse();
        response.Message.Should().NotBeNullOrEmpty();
        response.Code.Should().Be("TERMS_CONDITIONS_NOT_ACCEPTED");

        var walletBalanceList = await ToListAsync<UserWalletBalance>(x => true);
        walletBalanceList.Should().BeEmpty();
    }

    [Test]
    public async Task Should_Process_Free_Event_Successfully()
    {
        #region LoyaltyEngine service Arrange

        var campaign = await CreateCampaignMock();

        campaign.Rewards = campaign.Rewards.Where(r => r.IntegrationId == "RETRY").ToList();

        _loyaltyEngine = GetServiceMock<ILoyaltyEngine>();
        _loyaltyEngine.Setup(x => x.GetCampaign(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>()))
            .ReturnsAsync(campaign);

        var processEventResponse = new RewardByProbabilityResponseDto()
        {
            Success = true,
            Code = "OK",
            Effect = new LoyaltyEffectDto()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Evento procesado exitosamente",
                CampaignId = "anyId",
                Action = new LoyaltyEffectActionDto
                {
                    Type = "AddRetries",
                    Amount = 0,
                    Metadata = new Dictionary<string, string>()
                },
                Type = EffectTypeValue.Reward,
                Status = EffectStatus.Completed,
                Reward = new LoyaltyRewardDto()
                {
                    Id = Guid.NewGuid(),
                    IntegrationId = "RETRY",
                    Name = "Giro a la ruleta",
                    EffectSubType = EffectSubTypeValue.Retry
                }
            },
            Message = "Evento procesado exitosamente"
        };
        processEventResponse.Effect.Action.Metadata.TryAdd("transactionId", "C7BECA1F-C752-4AFF-41F9-08DCC2242E06");

        _loyaltyEngine.Setup(x => x.GetRewardByProbability(It.IsAny<RewardByProbabilityInputDto>()))
            .ReturnsAsync(processEventResponse);

        #endregion

        // process an event with cost to get a reward
        _walletsService = GetServiceMock<IWalletsService>();

        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0 });

        _walletsService.Setup(x => x.Debit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 250, Debit = 0 });

        _walletsService.Setup(x => x.Credit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 100, Debit = 0 });

        var commonServiceResponse = await SendAsync(new ProcessEventCommand { EventType = EventTypeValue.PlayGame, IsExtraAttempt = false });

        commonServiceResponse.Data!.Effect?.SubType.Should().Be(EffectSubTypeValue.Retry);

        var transaction = await FirstAsync<Transaction>(x => x.TransactionType == EffectTypeValue.Reward);
        transaction.TransactionSubType.Should().Be(EffectSubTypeValue.Retry);

        // process a free event redeeming the reward
        var freeEventResponse = await SendAsync(new ProcessEventCommand { EventType = EventTypeValue.PlayGame, IsExtraAttempt = false });

        freeEventResponse.Data!.EventCost.Should().Be(0);
        freeEventResponse.Data!.Balance.Before.Should().Be(500);
        // check the user wallet balance from the DB, should have only one debit transaction
        var walletBalanceList = await ToListAsync<UserWalletBalance>(x => x.Action == WalletActionValue.Debit);
        walletBalanceList.Should().HaveCount(1);
        // check transactions from the DB, should have two transactions, one unredeemed and one redeemed
        var transactions = await ToListAsync<Transaction>(x => x.TransactionType == EffectTypeValue.Reward);
        transactions.Should().HaveCount(2);
        transactions[0].TransactionSubType.Should().Be(EffectSubTypeValue.Retry);
        transactions[0].TransactionStatus.Should().Be(TransactionStatusValue.Redeemed);
        var secondTransaction = transactions[1];
        secondTransaction.TransactionSubType.Should().Be(EffectSubTypeValue.Retry);
        secondTransaction.TransactionStatus.Should().Be(TransactionStatusValue.Created);
        // check the metadata of the transactions, the second should have the redeemed reward id
        secondTransaction.Metadata.ContainsKey(TransactionMetadata.RedeemedTransactionId).Should().BeTrue();
        secondTransaction.Metadata.ContainsValue(transactions[0].Id.ToString()).Should().BeTrue();
    }

    [Test]
    public async Task Should_Fail_When_Process_Event_Fail_And_Is_A_Free_Event()
    {
        #region LoyaltyEngine service Arrange

        var campaign = await CreateCampaignMock();

        // modify campaign to have only rewards of type RETRY
        campaign.Rewards = campaign.Rewards.Where(r => r.IntegrationId == "RETRY").ToList();

        _loyaltyEngine = GetServiceMock<ILoyaltyEngine>();
        _loyaltyEngine.Setup(x => x.GetCampaign(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>()))
            .ReturnsAsync(campaign);

        var processEventResponse = new RewardByProbabilityResponseDto()
        {
            Success = true,
            Code = "OK",
            Effect = new LoyaltyEffectDto()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Giro de Ruleta",
                Type = EffectTypeValue.Reward,
                Status = EffectStatus.Completed,
                Action = new LoyaltyEffectActionDto
                {
                    Type = "AddRetries",
                    Amount = 1000,
                    Metadata = new Dictionary<string, string> { { "transactionId", "t-id" } }
                },
                CampaignId = "campaign-id",
                Reward = new LoyaltyRewardDto
                {
                    Id = Guid.NewGuid(),
                    IntegrationId = "RETRY",
                    Name = "Giro de Ruleta",
                    EffectSubType = EffectSubTypeValue.Retry
                }
            },
        };
        processEventResponse.Effect.Action.Metadata.TryAdd("transactionId", "C7BECA1F-C752-4AFF-41F9-08DCC2242E06");

        _loyaltyEngine.Setup(x => x.GetRewardByProbability(It.IsAny<RewardByProbabilityInputDto>()))
            .ReturnsAsync(processEventResponse);

        #endregion

        // process an event with cost to get a reward
        _walletsService = GetServiceMock<IWalletsService>();

        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0 });

        _walletsService.Setup(x => x.Debit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 250, Debit = 0 });

        _walletsService.Setup(x => x.Credit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 100, Debit = 0 });

        var commonServiceResponse = await SendAsync(new ProcessEventCommand { EventType = EventTypeValue.PlayGame, IsExtraAttempt = false });

        commonServiceResponse.Data!.Effect!.SubType.Should().Be(EffectSubTypeValue.Retry);

        var transaction = await FirstAsync<Transaction>(x => x.TransactionType == EffectTypeValue.Reward);
        transaction.TransactionSubType.Should().Be(EffectSubTypeValue.Retry);

        processEventResponse = new RewardByProbabilityResponseDto()
        {
            Success = false,
            Code = "BUDGET_EXCEEDED",
            Effect = new LoyaltyEffectDto()
            {
                Id = Guid.NewGuid().ToString(),
                CampaignId = "anyId",
                Action = new LoyaltyEffectActionDto() { Type = "AddRetries", Amount = 1000 },
                Type = EffectTypeValue.Reward,
                Status = EffectStatus.Completed,
            },
            Message = "Presupuesto excedido."
        };
        processEventResponse.Effect.Action.Metadata.TryAdd("transactionId", "C7BECA1F-C752-4AFF-41F9-08DCC2242E06");

        _loyaltyEngine.Setup(x => x.GetRewardByProbability(It.IsAny<RewardByProbabilityInputDto>()))
            .ReturnsAsync(processEventResponse);


        // try to process a free event redeeming the reward
        var freeEventResponse = await SendAsync(new ProcessEventCommand { EventType = EventTypeValue.PlayGame, IsExtraAttempt = false });

        // Check the response
        freeEventResponse.Success.Should().BeFalse();
        freeEventResponse.Message.Should().Be("Presupuesto excedido.");
        freeEventResponse.Code.Should().Be("BUDGET_EXCEEDED");
        freeEventResponse.Data.Should().BeNull();
        // check the user wallet balance from the DB, should have only one debit transaction
        var walletBalanceList = await ToListAsync<UserWalletBalance>(x => x.Action == WalletActionValue.Debit);
        walletBalanceList.Should().HaveCount(1);
        // check transactions from the DB, should have one transaction with status Created
        var transactions = await ToListAsync<Transaction>(x => true);
        transactions.Should().HaveCount(2);
        transactions[1].TransactionSubType.Should().Be(EffectSubTypeValue.Retry);
        transactions[1].TransactionStatus.Should().Be(TransactionStatusValue.Created);
    }

    [Test]
    public async Task Should_Fail_When_Process_Effect_Action_Fail_And_Is_A_Free_Event()
    {
        #region LoyaltyEngine service Arrange

        var campaign = await CreateCampaignMock();

        campaign.Rewards = campaign.Rewards.Where(r => r.IntegrationId == "RETRY" || r.IntegrationId == "POINT")
            .ToList();

        _loyaltyEngine = GetServiceMock<ILoyaltyEngine>();
        _loyaltyEngine.Setup(x => x.GetCampaign(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>()))
            .ReturnsAsync(campaign);

        var processEventResponse = new RewardByProbabilityResponseDto()
        {
            Success = true,
            Effect = new LoyaltyEffectDto()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Giro de Ruleta",
                Type = EffectTypeValue.Reward,
                Status = EffectStatus.Completed,
                Action = new LoyaltyEffectActionDto
                {
                    Type = "AddRetries",
                    Amount = 250,
                    Metadata = new Dictionary<string, string> { { "transactionId", "t-id" } }
                },
                CampaignId = "campaign-id",
                Reward = new LoyaltyRewardDto
                {
                    Id = Guid.NewGuid(),
                    IntegrationId = "RETRY",
                    Name = "Giro de Ruleta",
                    EffectSubType = EffectSubTypeValue.Retry
                }
            },
        };
        processEventResponse.Effect.Action.Metadata.TryAdd("transactionId", "C7BECA1F-C752-4AFF-41F9-08DCC2242E06");

        _loyaltyEngine.Setup(x => x.GetRewardByProbability(It.IsAny<RewardByProbabilityInputDto>()))
            .ReturnsAsync(processEventResponse);

        #endregion


        // process an event with cost to get a reward
        _walletsService = GetServiceMock<IWalletsService>();

        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0 });

        _walletsService.Setup(x => x.Debit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 250, Debit = 0 });

        _walletsService.Setup(x => x.Credit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 100, Debit = 0 });

        var commonServiceResponse = await SendAsync(new ProcessEventCommand { EventType = EventTypeValue.PlayGame, IsExtraAttempt = false });

        commonServiceResponse.Data!.Effect!.SubType.Should().Be(EffectSubTypeValue.Retry);

        var transaction = await FirstAsync<Transaction>(x => x.TransactionType == EffectTypeValue.Reward);
        transaction.TransactionSubType.Should().Be(EffectSubTypeValue.Retry);

        // Mock the WalletService to fail when crediting the reward
        _walletsService.Setup(x => x.Credit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(() => null);

        processEventResponse = new RewardByProbabilityResponseDto()
        {
            Success = false,
            Code = "REWARD_ERROR",
            Message = "Error al obtener recompensa"
        };

        _loyaltyEngine.Setup(x => x.GetRewardByProbability(It.IsAny<RewardByProbabilityInputDto>()))
            .ReturnsAsync(processEventResponse);

        // try to process a free event redeeming the reward
        var freeEventResponse = await SendAsync(new ProcessEventCommand { EventType = EventTypeValue.PlayGame, IsExtraAttempt = false });

        // Check the response
        freeEventResponse.Success.Should().BeFalse();
        freeEventResponse.Message.Should().Be("Error al obtener recompensa");
        freeEventResponse.Code.Should().Be("REWARD_ERROR");
        freeEventResponse.Data.Should().BeNull();

        // check the user wallet balance from the DB, should have only one debit transaction
        var walletBalanceList = await ToListAsync<UserWalletBalance>(x => x.Action == WalletActionValue.Debit);
        walletBalanceList.Should().HaveCount(1);
        // check transactions from the DB, should have one transaction with status Created
        var transactions = await ToListAsync<Transaction>(x => true);
        transactions.Should().HaveCount(2);
        transactions[1].TransactionSubType.Should().Be(EffectSubTypeValue.Retry);
        transactions[1].TransactionStatus.Should().Be(TransactionStatusValue.Created);
    }

    [Test]
    public async Task Should_Select_Oldest_Unredeemed_Transaction()
    {
        // process an event with cost to get a reward
        _walletsService = GetServiceMock<IWalletsService>();

        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0 });

        _walletsService.Setup(x => x.Debit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 250, Debit = 0 });

        _walletsService.Setup(x => x.Credit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 100, Debit = 0 });

        var user = await FirstAsync<User>(u => true);

        var transaction1 = new Transaction()
        {
            Id = Guid.Empty,
            Name = "Giro a la ruleta",
            TransactionStatus = TransactionStatusValue.Created,
            TransactionType = EffectTypeValue.Reward,
            TransactionSubType = EffectSubTypeValue.Retry,
            User = user,
            Metadata = new Dictionary<string, string>() { { TransactionMetadata.CampaignId, "CAMPAIGN-ID" } },
            RuleEffect = new RuleEffect { Id = "mockedRuleEffectId", }
        };

        var transaction2 = new Transaction()
        {
            Id = Guid.Empty,
            Name = "Giro a la ruleta",
            TransactionStatus = TransactionStatusValue.Created,
            TransactionType = EffectTypeValue.Reward,
            TransactionSubType = EffectSubTypeValue.Retry,
            User = user,
            Metadata = new Dictionary<string, string>() { { TransactionMetadata.CampaignId, "CAMPAIGN-ID" } },
            RuleEffect = new RuleEffect { Id = "mockedRuleEffectId", }
        };

        await AttachEntity(transaction1);

        await Task.Delay(1500);

        await AttachEntity(transaction2);

        _loyaltyEngine = GetServiceMock<ILoyaltyEngine>();

        var processEventResponse = new RewardByProbabilityResponseDto()
        {
            Success = true,
            Effect = new LoyaltyEffectDto()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Giro de Ruleta",
                Type = EffectTypeValue.Reward,
                Status = EffectStatus.Completed,
                Action = new LoyaltyEffectActionDto
                {
                    Type = "AddRetries",
                    Amount = 250,
                    Metadata = new Dictionary<string, string> { { "transactionId", "t-id" } }
                },
                CampaignId = "campaign-id",
                Reward = new LoyaltyRewardDto
                {
                    Id = Guid.NewGuid(),
                    IntegrationId = "RETRY",
                    Name = "Giro de Ruleta",
                    EffectSubType = EffectSubTypeValue.Retry
                }
            },
        };
        processEventResponse.Effect.Action.Metadata.TryAdd("transactionId", "C7BECA1F-C752-4AFF-41F9-08DCC2242E06");

        _loyaltyEngine.Setup(x => x.GetRewardByProbability(It.IsAny<RewardByProbabilityInputDto>()))
            .ReturnsAsync(processEventResponse);

        // process a free event redeeming the reward
        var freeEventResponse = await SendAsync(new ProcessEventCommand { EventType = EventTypeValue.PlayGame, IsExtraAttempt = false });

        freeEventResponse.Data!.EventCost.Should().Be(0);
        freeEventResponse.Data!.Balance.Before.Should().Be(500);
        // check transactions from the DB, should have three transactions, two unredeemed and one redeemed
        var transactions = await ToListAsync<Transaction>(x => true);
        transactions.Should().HaveCount(3);

        var redeemedTransaction =
            transactions.FirstOrDefault(t => t.TransactionStatus == TransactionStatusValue.Redeemed);
        redeemedTransaction!.Id.Should().Be(transaction1.Id);

        transactions.FirstOrDefault(t => t.Id == transaction2.Id)!.TransactionStatus.Should()
            .Be(TransactionStatusValue.Created);
    }

    [Test]
    public async Task Should_Apply_Cash_Back_When_Reward_Is_CashBack_Type()
    {
        #region LoyaltyEngine service Arrange

        var campaign = await CreateCampaignMock();

        _loyaltyEngine = GetServiceMock<ILoyaltyEngine>();
        _loyaltyEngine.Setup(x => x.GetCampaign(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>()))
            .ReturnsAsync(campaign);

        var processEventResponse = new RewardByProbabilityResponseDto()
        {
            Success = true,
            Effect = new LoyaltyEffectDto()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Cashback en tarjeta n1co",
                Type = EffectTypeValue.Reward,
                Status = EffectStatus.Completed,
                Action = new LoyaltyEffectActionDto
                {
                    Type = "AddCash",
                    Amount = 4,
                    Metadata = new Dictionary<string, string> { { "transactionId", "t-id" } }
                },
                CampaignId = "campaign-id",
                Reward = new LoyaltyRewardDto
                {
                    Id = Guid.NewGuid(),
                    IntegrationId = "CASH",
                    Name = "Cashback en tarjeta n1co",
                    EffectSubType = EffectSubTypeValue.Cash
                }
            },
        };
        processEventResponse.Effect.Action.Metadata.TryAdd("transactionId", "C7BECA1F-C752-4AFF-41F9-08DCC2242E06");

        _loyaltyEngine.Setup(x => x.GetRewardByProbability(It.IsAny<RewardByProbabilityInputDto>()))
            .ReturnsAsync(processEventResponse);

        #endregion

        #region WalletServiceMock Arrange

        // process an event with cost to get a reward
        _walletsService = GetServiceMock<IWalletsService>();

        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0 });

        _walletsService.Setup(x => x.Debit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 250, Debit = 0 });

        _walletsService.Setup(x => x.Credit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 100, Debit = 0 });

        #endregion

        // try to process a free event redeeming the reward
        var processEventCommandResponse =
            await SendAsync(new ProcessEventCommand { EventType = EventTypeValue.PlayGame, IsExtraAttempt = false });

        processEventCommandResponse.Success.Should().BeTrue();
        processEventCommandResponse.Data!.Effect!.SubType.Should().Be(EffectSubTypeValue.Cash);
        processEventCommandResponse.Data.Effect.Type.Should().Be(EffectTypeValue.Reward);
        processEventCommandResponse.Message.Should().Be("Ruleta girada exitosamente");
        processEventCommandResponse.Code.Should().Be("OK");

        var cashBackServiceMock = GetServiceMock<ICashBackService>();
        cashBackServiceMock.Verify(
            x => x.ApplyCashBack(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);

        var transaction = await FirstAsync<Transaction>(x => x.TransactionType == EffectTypeValue.Reward);
        transaction.TransactionSubType.Should().Be(EffectSubTypeValue.Cash);
        transaction.Description.Should().Be(TransactionDescription.Roulette);
        transaction.Name.Should().Be(TransactionName.CashbackReward);

        transaction.Metadata.Should().NotBeEmpty();
        transaction.Metadata.Should().ContainKey(TransactionMetadata.IssuingAppliedCashBackTransactionId);
        transaction.Metadata[TransactionMetadata.IssuingAppliedCashBackTransactionId].Should()
            .Be("appliedCashBackTransactionId");
        transaction.Metadata.Should().ContainKey(TransactionMetadata.IssuingCashBackTransactionId);
        transaction.Metadata[TransactionMetadata.IssuingCashBackTransactionId].Should()
            .Be("cashBackTransactionId");
        transaction.Metadata.Should().ContainKey(TransactionMetadata.IssuingCashBackTransactionAmount);
        transaction.Metadata[TransactionMetadata.IssuingCashBackTransactionAmount].Should()
            .Be("100");
    }

    [Test]
    public async Task Should_Apply_Cash_Back_When_Reward_Is_CashBack_Type_Without_Metadata()
    {
        #region LoyaltyEngine service Arrange

        var campaign = await CreateCampaignMock();

        _loyaltyEngine = GetServiceMock<ILoyaltyEngine>();
        _loyaltyEngine.Setup(x => x.GetCampaign(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>()))
            .ReturnsAsync(campaign);

        var processEventResponse = new RewardByProbabilityResponseDto()
        {
            Success = true,
            Effect = new LoyaltyEffectDto()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Cashback en tarjeta n1co",
                Type = EffectTypeValue.Reward,
                Status = EffectStatus.Completed,
                Action = new LoyaltyEffectActionDto
                {
                    Type = "AddCash",
                    Amount = 4,
                    Metadata = new Dictionary<string, string> { { "transactionId", "t-id" } }
                },
                CampaignId = "campaign-id",
                Reward = new LoyaltyRewardDto
                {
                    Id = Guid.NewGuid(),
                    IntegrationId = "CASH",
                    Name = "Cashback en tarjeta n1co",
                    EffectSubType = EffectSubTypeValue.Cash
                }
            },
        };
        processEventResponse.Effect.Action.Metadata.TryAdd("transactionId", "C7BECA1F-C752-4AFF-41F9-08DCC2242E06");

        _loyaltyEngine.Setup(x => x.GetRewardByProbability(It.IsAny<RewardByProbabilityInputDto>()))
            .ReturnsAsync(processEventResponse);

        #endregion

        #region WalletServiceMock Arrange

        // process an event with cost to get a reward
        _walletsService = GetServiceMock<IWalletsService>();

        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0 });

        _walletsService.Setup(x => x.Debit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 250, Debit = 0 });

        _walletsService.Setup(x => x.Credit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 100, Debit = 0 });

        #endregion

        var cashBackServiceMock = GetServiceMock<ICashBackService>();

        cashBackServiceMock
            .Setup(x => x.ApplyCashBack(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new ApplyCashBackDto()
            {
                Success = true,
                Code = "OK",
                Message = "Cashback aplicado exitosamente",
                CashBackTransaction = new CashBackTransactionDto()
            });

        // try to process a free event redeeming the reward
        var processEventCommandResponse =
            await SendAsync(new ProcessEventCommand { EventType = EventTypeValue.PlayGame, IsExtraAttempt = false });

        processEventCommandResponse.Success.Should().BeTrue();
        processEventCommandResponse.Data!.Effect!.SubType.Should().Be(EffectSubTypeValue.Cash);
        processEventCommandResponse.Data.Effect.Type.Should().Be(EffectTypeValue.Reward);
        processEventCommandResponse.Message.Should().Be("Ruleta girada exitosamente");
        processEventCommandResponse.Code.Should().Be("OK");


        cashBackServiceMock.Verify(
            x => x.ApplyCashBack(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);

        var transaction = await FirstAsync<Transaction>(x => x.TransactionType == EffectTypeValue.Reward);
        transaction.TransactionSubType.Should().Be(EffectSubTypeValue.Cash);
        transaction.Description.Should().Be(TransactionDescription.Roulette);
        transaction.Name.Should().Be(TransactionName.CashbackReward);

        transaction.Metadata.Should().NotBeEmpty();
        transaction.Metadata.Should().ContainKey(TransactionMetadata.IssuingAppliedCashBackTransactionId);
        transaction.Metadata[TransactionMetadata.IssuingAppliedCashBackTransactionId].Should()
            .Be("");
        transaction.Metadata.Should().ContainKey(TransactionMetadata.IssuingCashBackTransactionId);
        transaction.Metadata[TransactionMetadata.IssuingCashBackTransactionId].Should()
            .Be("");
        transaction.Metadata.Should().ContainKey(TransactionMetadata.IssuingCashBackTransactionAmount);
        transaction.Metadata[TransactionMetadata.IssuingCashBackTransactionAmount].Should()
            .Be("");
    }

    [Test]
    public async Task Should_Return_Error_When_Dont_Apply_CashBack()
    {
        #region LoyaltyEngine service Arrange

        var campaign = await CreateCampaignMock();

        _loyaltyEngine = GetServiceMock<ILoyaltyEngine>();
        _loyaltyEngine.Setup(x => x.GetCampaign(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>()))
            .ReturnsAsync(campaign);

        var processEventResponse = new RewardByProbabilityResponseDto()
        {
            Success = true,
            Effect = new LoyaltyEffectDto()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Cashback en tarjeta n1co",
                Type = EffectTypeValue.Reward,
                Status = EffectStatus.Completed,
                Action = new LoyaltyEffectActionDto
                {
                    Type = "AddCash",
                    Amount = 4,
                    Metadata = new Dictionary<string, string> { { "transactionId", "t-id" } }
                },
                CampaignId = "campaign-id",
                Reward = new LoyaltyRewardDto
                {
                    Id = Guid.NewGuid(),
                    IntegrationId = "CASH",
                    Name = "Cashback en tarjeta n1co",
                    EffectSubType = EffectSubTypeValue.Cash
                }
            },
        };
        processEventResponse.Effect.Action.Metadata.TryAdd("transactionId", "C7BECA1F-C752-4AFF-41F9-08DCC2242E06");

        _loyaltyEngine.Setup(x => x.GetRewardByProbability(It.IsAny<RewardByProbabilityInputDto>()))
            .ReturnsAsync(processEventResponse);

        #endregion

        #region WalletServiceMock Arrange

        // process an event with cost to get a reward
        _walletsService = GetServiceMock<IWalletsService>();

        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0, TransactionId = "f181aa94-838a-418f-83e5-ed7f4bc1254g" });

        _walletsService.Setup(x => x.Debit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 250, Debit = 0, TransactionId = "f181aa94-838a-418f-83e5-ed7f4bc1253d" });

        _walletsService.Setup(x => x.Credit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 100, Debit = 0, TransactionId = "f181aa94-838a-418f-83e5-ed7f4bc1255d" });

        _walletsService.Setup(x => x.Void(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 250, Debit = 0, TransactionId = "f181aa94-838a-418f-83e5-ed7f4bc1259d" });

        #endregion

        var cashBackServiceMock = GetServiceMock<ICashBackService>();
        cashBackServiceMock
            .Setup(x => x.ApplyCashBack(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new ApplyCashBackDto()
            {
                Success = false,
                Message = "Error al aplicar cashback",
                Code = "Error"
            });

        // try to process a free event redeeming the reward
        var processEventCommandResponse =
            await SendAsync(new ProcessEventCommand { EventType = EventTypeValue.PlayGame, IsExtraAttempt = false });

        processEventCommandResponse.Success.Should().BeFalse();
        processEventCommandResponse.Data.Should().BeNull();
        processEventCommandResponse.Message.Should().Be("Error general: No se pudo acreditar el premio");
        processEventCommandResponse.Code.Should().Be("GENERAL");

        cashBackServiceMock.Verify(
            x => x.ApplyCashBack(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);

        var transactions = await AllToListAsync<Transaction>();
        transactions.Count.Should().Be(2);
        transactions.Should().Contain(x => x.TransactionType == EffectTypeValue.Debit).And
            .Contain(x => x.TransactionType == EffectTypeValue.Refund);

        var debitTransaction = transactions.First(x => x.TransactionType == EffectTypeValue.Debit);
        var refundTransaction = transactions.First(x => x.TransactionType == EffectTypeValue.Refund);

        debitTransaction.TransactionStatus.Should().Be(TransactionStatusValue.Voided);
        debitTransaction.Metadata.ContainsKey(TransactionMetadata.VoidTransactionId).Should().BeTrue();
        debitTransaction.Metadata.ContainsValue(refundTransaction.Id.ToString()).Should().BeTrue();

        refundTransaction.TransactionStatus.Should().Be(TransactionStatusValue.Redeemed);
        refundTransaction.Metadata.ContainsKey(TransactionMetadata.VoidedTransactionId).Should().BeTrue();
        refundTransaction.Metadata.ContainsValue(debitTransaction.Id.ToString()).Should().BeTrue();
    }

    [Test]
    public async Task Should_Return_Error_When_Dont_Apply_CashBack_And_Is_A_Free_Event()
    {
        #region WalletServiceMock Arrange

        // process an event with cost to get a reward
        _walletsService = GetServiceMock<IWalletsService>();

        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0 });

        _walletsService.Setup(x => x.Debit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 250, Debit = 0 });

        _walletsService.Setup(x => x.Credit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 100, Debit = 0 });

        #endregion

        #region SetAFreeEvent

        var campaign = await CreateCampaignMock();

        _loyaltyEngine = GetServiceMock<ILoyaltyEngine>();
        _loyaltyEngine.Setup(x => x.GetCampaign(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>()))
            .ReturnsAsync(campaign);

        var rewardByProbabilityResponseDto = new RewardByProbabilityResponseDto()
        {
            Success = true,
            Effect = new LoyaltyEffectDto()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Un intento gratis",
                Type = EffectTypeValue.Reward,
                Status = EffectStatus.Completed,
                Action = new LoyaltyEffectActionDto
                {
                    Type = "AddRetries",
                    Amount = 250,
                    Metadata = new Dictionary<string, string> { { "transactionId", "t-id" } }
                },
                CampaignId = "campaign-id",
                Reward = new LoyaltyRewardDto
                {
                    Id = Guid.NewGuid(),
                    IntegrationId = "RETRY",
                    Name = "Giro Gratis",
                    EffectSubType = EffectSubTypeValue.Retry
                }
            },
        };
        rewardByProbabilityResponseDto.Effect.Action.Metadata.TryAdd("transactionId", "C7BECA1F-C752-4AFF-41F9-08DCC2242E06");

        _loyaltyEngine.Setup(x => x.GetRewardByProbability(It.IsAny<RewardByProbabilityInputDto>()))
            .ReturnsAsync(rewardByProbabilityResponseDto);

        await SendAsync(new ProcessEventCommand { EventType = EventTypeValue.PlayGame, IsExtraAttempt = false });

        #endregion

        #region LoyaltyEngineService

        var processEventResponse = new RewardByProbabilityResponseDto()
        {
            Success = true,
            Effect = new LoyaltyEffectDto()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Cashback en tarjeta n1co",
                Type = EffectTypeValue.Reward,
                Status = EffectStatus.Completed,
                Action = new LoyaltyEffectActionDto
                {
                    Type = "AddCash",
                    Amount = 4,
                    Metadata = new Dictionary<string, string> { { "transactionId", "t-id" } }
                },
                CampaignId = "campaign-id",
                Reward = new LoyaltyRewardDto
                {
                    Id = Guid.NewGuid(),
                    IntegrationId = "CASH",
                    Name = "Cashback en tarjeta n1co",
                    EffectSubType = EffectSubTypeValue.Cash
                }
            },
        };
        processEventResponse.Effect.Action.Metadata.TryAdd("transactionId", "C7BECA1F-C752-4AFF-41F9-08DCC2242E06");

        _loyaltyEngine.Setup(x => x.GetRewardByProbability(It.IsAny<RewardByProbabilityInputDto>()))
            .ReturnsAsync(processEventResponse);

        #endregion

        var cashBackServiceMock = GetServiceMock<ICashBackService>();
        cashBackServiceMock
            .Setup(x => x.ApplyCashBack(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new ApplyCashBackDto()
            {
                Success = false,
                Message = "Error al aplicar cashback",
                Code = "Error"
            });

        // try to process a free event redeeming the reward
        var processEventCommandResponse =
            await SendAsync(new ProcessEventCommand { EventType = EventTypeValue.PlayGame, IsExtraAttempt = false });

        processEventCommandResponse.Success.Should().BeFalse();
        processEventCommandResponse.Data.Should().BeNull();
        processEventCommandResponse.Message.Should().Be("Error general: No se pudo acreditar el premio");
        processEventCommandResponse.Code.Should().Be("GENERAL");

        cashBackServiceMock.Verify(
            x => x.ApplyCashBack(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);

        //Only should have a debit transaction and a reward transaction (Because the second try was free)
        var transactions = await AllToListAsync<Transaction>();
        transactions.Count.Should().Be(2);
        transactions.Should().Contain(x => x.TransactionType == EffectTypeValue.Debit).And
            .Contain(x => x.TransactionType == EffectTypeValue.Reward);

        //The reward transaction not should be redeemed
        transactions.FirstOrDefault(x => x.TransactionType == EffectTypeValue.Reward)!.TransactionStatus.Should().Be(TransactionStatusValue.Created);
    }

    [Test]
    public async Task Should_Not_Process_Event_With_ExtraAttempt_When_Have_remainingAttempts()
    {
        // Arrange

        #region LoyaltyEngine service Arrange

        var campaign = await CreateCampaignMock();
        // update campaign event frequency limit
        campaign.UserEventFrequencyLimit = 1;

        _loyaltyEngine = GetServiceMock<ILoyaltyEngine>();
        _loyaltyEngine.Setup(x => x.GetCampaign(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>()))
            .ReturnsAsync(campaign);

        var processEventResponse = new RewardByProbabilityResponseDto()
        {
            Success = true,
            Code = "OK",
            Effect = new LoyaltyEffectDto()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Has acumulado co1ns",
                Type = EffectTypeValue.Reward,
                Status = EffectStatus.Completed,
                Action = new LoyaltyEffectActionDto
                {
                    Type = "AddPoints",
                    Amount = 1000,
                    Metadata = new Dictionary<string, string> { { "transactionId", "t-id" } }
                },
                CampaignId = "campaign-id",
                Reward = new LoyaltyRewardDto
                {
                    Id = Guid.NewGuid(),
                    IntegrationId = "POINT",
                    Name = "Has acumulado co1ns"
                }
            },
        };
        processEventResponse.Effect.Action.Metadata.TryAdd("transactionId", "C7BECA1F-C752-4AFF-41F9-08DCC2242E06");

        _loyaltyEngine.Setup(x => x.GetRewardByProbability(It.IsAny<RewardByProbabilityInputDto>()))
            .ReturnsAsync(processEventResponse);

        #endregion

        // Arrange
        _walletsService = GetServiceMock<IWalletsService>();

        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 5000, Debit = 0 });

        _walletsService.Setup(x => x.Debit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 250, Debit = 0 });

        _walletsService.Setup(x => x.Credit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 100, Debit = 0 });

        // Act
        var command = new ProcessEventCommand { EventType = EventTypeValue.PlayGame, IsExtraAttempt = true };

        // Assert
        var exceptionAssertions = await FluentActions.Invoking(() =>
                SendAsync(command))
            .Should()
            .ThrowAsync<ValidationException>();

        var errors = exceptionAssertions.Which.Errors;
        errors.Should().HaveCount(1);
        errors.Should().ContainKey("");
        errors[""].Should().Contain("No puedes realizar un intento extra.");
    }

    [Test]
    public async Task Should_Process_Event_With_ExtraAttempt_When_RemainingAttempts_Is_Zero_and_Balance_IsGreater_To_ExtraAttempCost()
    {
        // Arrange

        #region LoyaltyEngine service Arrange

        var campaign = await CreateCampaignMock();
        // update campaign event frequency limit
        campaign.UserEventFrequencyLimit = 1;

        _loyaltyEngine = GetServiceMock<ILoyaltyEngine>();
        _loyaltyEngine.Setup(x => x.GetCampaign(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>()))
            .ReturnsAsync(campaign);

        var processEventResponse = new RewardByProbabilityResponseDto()
        {
            Success = true,
            Code = "OK",
            Effect = new LoyaltyEffectDto()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Has ganado coins",
                Type = EffectTypeValue.Reward,
                Status = EffectStatus.Completed,
                Action = new LoyaltyEffectActionDto
                {
                    Type = "AddPoints",
                    Amount = 1000,
                    Metadata = new Dictionary<string, string> { { "transactionId", "t-id" } }
                },
                CampaignId = "campaign-id",
                Reward = new LoyaltyRewardDto
                {
                    Id = Guid.NewGuid(),
                    IntegrationId = "POINT",
                    Name = "Has ganado coins",
                    EffectSubType = EffectSubTypeValue.Point
                }
            },
            Message = "Evento procesado exitosamente"
        };
        processEventResponse.Effect.Action.Metadata.TryAdd("transactionId", "C7BECA1F-C752-4AFF-41F9-08DCC2242E06");

        _loyaltyEngine.Setup(x => x.GetRewardByProbability(It.IsAny<RewardByProbabilityInputDto>()))
            .ReturnsAsync(processEventResponse);

        #endregion

        // Arrange
        _walletsService = GetServiceMock<IWalletsService>();

        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 5000, Debit = 0 });

        _walletsService.Setup(x => x.Debit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 250, Debit = 0 });

        _walletsService.Setup(x => x.Credit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 100, Debit = 0 });

        // Act
        var command = new ProcessEventCommand { EventType = EventTypeValue.PlayGame, IsExtraAttempt = false };

        // Trigger the first event
        await SendAsync(command);

        command = new ProcessEventCommand { EventType = EventTypeValue.PlayGame, IsExtraAttempt = true };

        var processEventCommandResponse = await SendAsync(command);

        processEventCommandResponse.Success.Should().BeTrue();
        processEventCommandResponse.Data!.Effect!.SubType.Should().Be(EffectSubTypeValue.Point);
        processEventCommandResponse.Data.Effect.Type.Should().Be(EffectTypeValue.Reward);
        processEventCommandResponse.Message.Should().Be("Ruleta girada exitosamente");
        processEventCommandResponse.Code.Should().Be("OK");

        // check transactions for rewards
        var transactions = await ToListAsync<Transaction>(x => x.TransactionType == EffectTypeValue.Debit);
        transactions.Should().HaveCount(2);
        transactions[0].TransactionSubType.Should().Be(EffectSubTypeValue.Point);
        transactions[0].TransactionStatus.Should().Be(TransactionStatusValue.Redeemed);
        transactions[0].Name.Should().Be(TransactionName.RouletteSpinDebit);

        var secondTransaction = transactions[1];
        secondTransaction.TransactionSubType.Should().Be(EffectSubTypeValue.Point);
        secondTransaction.TransactionStatus.Should().Be(TransactionStatusValue.Redeemed);
        secondTransaction.Name.Should().Be(TransactionName.RouletteExtraSpinDebit);
    }

    [Test]
    public async Task Should_Not_Process_Event_With_ExtraAttempt_When_Balance_Is_Less_To_ExtraAttempCost()
    {
        // Arrange

        #region LoyaltyEngine service Arrange

        var campaign = await CreateCampaignMock();
        // update campaign event frequency limit
        campaign.UserEventFrequencyLimit = 1;

        _loyaltyEngine = GetServiceMock<ILoyaltyEngine>();
        _loyaltyEngine.Setup(x => x.GetCampaign(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>()))
            .ReturnsAsync(campaign);

        var processEventResponse = new RewardByProbabilityResponseDto()
        {
            Success = true,
            Code = "OK",
            Effect = new LoyaltyEffectDto()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Has acumulado co1ns",
                Type = EffectTypeValue.Reward,
                Status = EffectStatus.Completed,
                Action = new LoyaltyEffectActionDto
                {
                    Type = "AddPoints",
                    Amount = 1000,
                    Metadata = new Dictionary<string, string> { { "transactionId", "t-id" } }
                },
                CampaignId = "campaign-id",
                Reward = new LoyaltyRewardDto
                {
                    Id = Guid.NewGuid(),
                    IntegrationId = "POINT",
                    Name = "Has acumulado co1ns"
                }
            },
        };
        processEventResponse.Effect.Action.Metadata.TryAdd("transactionId", "C7BECA1F-C752-4AFF-41F9-08DCC2242E06");

        _loyaltyEngine.Setup(x => x.GetRewardByProbability(It.IsAny<RewardByProbabilityInputDto>()))
            .ReturnsAsync(processEventResponse);

        #endregion

        // Arrange
        _walletsService = GetServiceMock<IWalletsService>();

        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 5000, Debit = 0 });

        _walletsService.Setup(x => x.Debit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 250, Debit = 0 });

        _walletsService.Setup(x => x.Credit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 100, Debit = 0 });

        // Act
        var command = new ProcessEventCommand { EventType = EventTypeValue.PlayGame, IsExtraAttempt = false };

        // Trigger the first event
        await SendAsync(command);

        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 200, Debit = 0 });

        command = new ProcessEventCommand { EventType = EventTypeValue.PlayGame, IsExtraAttempt = true };

        var exceptionAssertions = await FluentActions.Invoking(() =>
        SendAsync(command))
        .Should()
     .ThrowAsync<ValidationException>();

        var errors = exceptionAssertions.Which.Errors;
        errors.Should().HaveCount(1);
        errors.Should().ContainKey("");
        errors[""].Should().Contain("No puedes realizar un intento extra.");
    }

}
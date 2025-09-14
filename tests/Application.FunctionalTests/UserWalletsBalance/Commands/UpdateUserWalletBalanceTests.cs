using N1coLoyalty.Application.Users.Commands;
using N1coLoyalty.Domain.Entities;
using N1coLoyalty.Application.Common.Constants;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.TermsConditions.Commands;
using N1coLoyalty.Application.UserWalletBalances.Commands.UpdateUserWalletBalance;
using N1coLoyalty.Domain.Enums;
using N1coLoyalty.Application.Common.Exceptions;
using N1coLoyalty.Application.UserWalletBalances.Commands;

namespace N1coLoyalty.Application.FunctionalTests.UserWalletsBalance.Commands;

using static Testing;

public class UpdateUserWalletBalanceTests : BaseTestFixture
{
    private Mock<IWalletsService> _walletsService = new();
    private string _userPhone;

    [SetUp]
    public async Task Setup()
    {
        // Service Mock
        _walletsService = GetServiceMock<IWalletsService>();

        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0, HistoricalCredit = 500 });

        _walletsService.Setup(x => x.Credit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0, HistoricalCredit = 500 });

        _walletsService.Setup(x => x.CreateWallet(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0, HistoricalCredit = 500 });

        // Create User
        await SendAsync(new CreateUserCommand());

        // create a new terms and conditions in the database
        var termsConditions = new TermsConditionsInfo()
        {
            Id = Guid.NewGuid(),
            Version = "1.0.0",
            Url = "https://n1co.com/terminos-y-condiciones/",
            IsCurrent = true
        };

        // save the terms and conditions in the database
        await AddAsync(termsConditions);

        // call the command to accept the terms and conditions
        var acceptTermsConditionsCommand = new AcceptTermsConditionsCommand
        {
            IsAccepted = true
        };
        await SendAsync(acceptTermsConditionsCommand);

        // Get User Id
        var user = await FirstAsync<User>(x => true);
        user.Phone = "+573002222222";
        await UpdateAsync(user);
        _userPhone = user.Phone ?? "";
    }

    [Test]
    public async Task Should_Update_UserWalletBalances_With_Credit_Operation()
    {
        // arrange
        const int amount = 100;
        const string reason = "reason";
        const WalletOperationValue action = WalletOperationValue.Credit;
        const string reference = "f181aa94-838a-418f-83e5-ed7f4bc1253c";

        var walletBalanceResponseDto = new WalletBalanceResponseDto
        {
            Credit = 500m, Debit = 0, TransactionId = reference, HistoricalCredit = 1000
        };
        _walletsService.Setup(x => x.Credit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(walletBalanceResponseDto);

        var command = new UpdateUserWalletBalanceCommand
        {
            UserPhone = _userPhone, Operation = action, Reason = reason, Amount = amount,
        };

        // action
        var result = await SendAsync(command);
        result.Success.Should().BeTrue();
        result.Code.Should().Be("OK");
        result.Message.Should().NotBeNullOrEmpty();
        result.Data.Should().NotBeNull();
        result.Data?.Balance.Should().Be(walletBalanceResponseDto.Credit - walletBalanceResponseDto.Debit);
        result.Data?.HistoricalCredit.Should().Be(walletBalanceResponseDto.HistoricalCredit);

        // assert
        var transactionId = result.Data?.TransactionId;
        var transactionDb = await FirstAsync<Transaction>(x =>
            x.Id == transactionId && x.TransactionOrigin == TransactionOriginValue.Admin &&
            x.TransactionStatus == TransactionStatusValue.Redeemed &&
            x.TransactionSubType == EffectSubTypeValue.Point && x.Amount == amount &&
            x.Name == TransactionName.UpdateUserWalletBalance && x.Description == reason &&
            x.TransactionType == GetAction(action));
        transactionDb.Should().NotBeNull();

        var userWalletBalanceReference = result.Data?.Reference;
        var userWalletBalanceDb = await FirstAsync<UserWalletBalance>(x =>
            x.Reference == userWalletBalanceReference && x.Action == WalletActionValue.Credit && x.Amount == amount &&
            x.Reason == transactionDb.Name && x.Reference == reference && x.TransactionId == transactionDb.Id);
        userWalletBalanceDb.Should().NotBeNull();
    }

    [Test]
    public async Task Should_Update_UserWalletBalances_With_Debit_Operation()
    {
        // arrange
        const int amount = 100;
        const string reason = "reason";
        const WalletOperationValue action = WalletOperationValue.Debit;
        const string reference = "f181aa94-838a-418f-83e5-ed7f4bc1253c";

        var walletBalanceResponseDto = new WalletBalanceResponseDto
        {
            Credit = 0m, Debit = 500, TransactionId = reference, HistoricalCredit = 1000
        };
        _walletsService.Setup(x => x.Debit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(walletBalanceResponseDto);

        var command = new UpdateUserWalletBalanceCommand
        {
            UserPhone = _userPhone, Operation = action, Reason = reason, Amount = amount,
        };

        // action
        var result = await SendAsync(command);
        result.Code.Should().Be("OK");
        result.Message.Should().NotBeNullOrEmpty();
        result.Data.Should().NotBeNull();
        result.Data?.Balance.Should().Be(walletBalanceResponseDto.Credit - walletBalanceResponseDto.Debit);
        result.Data?.HistoricalCredit.Should().Be(walletBalanceResponseDto.HistoricalCredit);

        // assert
        var transactionId = result.Data?.TransactionId;
        var transactionDb = await FirstAsync<Transaction>(x => x.Id == transactionId &&
                                                               x.TransactionOrigin == TransactionOriginValue.Admin &&
                                                               x.TransactionStatus == TransactionStatusValue.Redeemed &&
                                                               x.TransactionSubType == EffectSubTypeValue.Point &&
                                                               x.Amount == amount &&
                                                               x.Name == TransactionName.UpdateUserWalletBalance &&
                                                               x.Description == reason &&
                                                               x.TransactionType == GetAction(action));
        transactionDb.Should().NotBeNull();

        var userWalletBalanceReference = result.Data?.Reference;
        var userWalletBalanceDb = await FirstAsync<UserWalletBalance>(x =>
            x.Reference == userWalletBalanceReference && x.Action == WalletActionValue.Debit && x.Amount == amount &&
            x.Reason == transactionDb.Name && x.Reference == reference && x.TransactionId == transactionDb.Id);
        userWalletBalanceDb.Should().NotBeNull();
    }

    [Test]
    public async Task Should_ThrowException_When_UserPhone_IsEmpty()
    {
        // arrange
        const int amount = 100;
        const string reason = "reason";
        const WalletOperationValue action = WalletOperationValue.Credit;
        const string reference = "f181aa94-838a-418f-83e5-ed7f4bc1253c";
        _userPhone = "";

        var walletBalanceResponseDto = new WalletBalanceResponseDto
        {
            Credit = 500m, Debit = 0, TransactionId = reference, HistoricalCredit = 1000
        };
        _walletsService.Setup(x => x.Credit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(walletBalanceResponseDto);

        var command = new UpdateUserWalletBalanceCommand
        {
            UserPhone = _userPhone, Operation = action, Reason = reason, Amount = amount,
        };

        // action
        var exceptionAssertions = await FluentActions.Invoking(() =>
                SendAsync(command))
            .Should()
            .ThrowAsync<ValidationException>();

        //assert
        var errors = exceptionAssertions.Which.Errors;
        errors.Should().HaveCount(1);
        errors.Should().ContainKey("UserPhone");
        errors["UserPhone"].Should().Contain("El número del usuario es requerido");
    }

    [Test]
    public async Task Should_ThrowException_When_Reason_IsEmpty()
    {
        // arrange
        const int amount = 100;
        const string reason = "";
        const WalletOperationValue action = WalletOperationValue.Credit;
        const string reference = "f181aa94-838a-418f-83e5-ed7f4bc1253c";

        var walletBalanceResponseDto = new WalletBalanceResponseDto
        {
            Credit = 500m, Debit = 0, TransactionId = reference, HistoricalCredit = 1000
        };
        _walletsService.Setup(x => x.Credit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(walletBalanceResponseDto);

        var command = new UpdateUserWalletBalanceCommand
        {
            UserPhone = _userPhone, Operation = action, Reason = reason, Amount = amount,
        };

        // action
        var exceptionAssertions = await FluentActions.Invoking(() =>
                SendAsync(command))
            .Should()
            .ThrowAsync<ValidationException>();

        var errors = exceptionAssertions.Which.Errors;
        errors.Should().HaveCount(1);
        errors.Should().ContainKey("Reason");
        errors["Reason"].Should().Contain("La razón es requerida");

    }

    [Test]
    public async Task Should_ThrowException_When_Amount_IsZero()
    {
        // arrange
        const int amount = 0;
        const string reason = "reason";
        const WalletOperationValue action = WalletOperationValue.Credit;
        const string reference = "f181aa94-838a-418f-83e5-ed7f4bc1253c";

        var walletBalanceResponseDto = new WalletBalanceResponseDto
        {
            Credit = 500m, Debit = 0, TransactionId = reference, HistoricalCredit = 1000
        };
        _walletsService.Setup(x => x.Credit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(walletBalanceResponseDto);

        var command = new UpdateUserWalletBalanceCommand
        {
            UserPhone = _userPhone, Operation = action, Reason = reason, Amount = amount,
        };

        // action
        var exceptionAssertions = await FluentActions.Invoking(() =>
                SendAsync(command))
            .Should()
            .ThrowAsync<ValidationException>();

        var errors = exceptionAssertions.Which.Errors;
        errors.Should().HaveCount(1);
        errors.Should().ContainKey("Amount");
        errors["Amount"].Should().Contain("El monto debe ser mayor a 0");

    }

    [Test]
    public async Task Should_ThrowException_When_Update_UserWalletBalances_And_UserIsUnknown()
    {
        // arrange
        const int amount = 100;
        const string reason = "reason";
        const WalletOperationValue action = WalletOperationValue.Debit;
        const string reference = "f181aa94-838a-418f-83e5-ed7f4bc1253c";

        var walletBalanceResponseDto = new WalletBalanceResponseDto
        {
            Credit = 0m, Debit = 500, TransactionId = reference, HistoricalCredit = 1000
        };
        _walletsService.Setup(x => x.Debit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(walletBalanceResponseDto);

        _userPhone = "1234567890";

        var command = new UpdateUserWalletBalanceCommand
        {
            UserPhone = _userPhone, Operation = action, Reason = reason, Amount = amount,
        };

        // action
        var exceptionAssertions = await FluentActions.Invoking(() =>
                SendAsync(command))
            .Should()
            .ThrowAsync<ValidationException>();

        var errors = exceptionAssertions.Which.Errors;
        errors.Should().HaveCount(1);
        errors.Should().ContainKey("UserPhone");
        errors["UserPhone"].Should().Contain("El usuario no existe");
    }

    [Test]
    public async Task Should_ThrowException_When_Update_UserWalletBalances_And_WalletResponseIsNull()
    {
        // arrange
        const int amount = 100;
        const string reason = "reason";
        const WalletOperationValue action = WalletOperationValue.Debit;

        _walletsService.Setup(x => x.CreateWallet(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0, HistoricalCredit = 500 });

        _walletsService.Setup(x => x.Debit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(() => null);

        var command = new UpdateUserWalletBalanceCommand
        {
            UserPhone = _userPhone, Operation = action, Reason = reason, Amount = amount,
        };
            
        // action   
        var result = await SendAsync(command);
        result.Success.Should().BeFalse();
        result.Code.Should().Be("ERROR");
        result.Message.Should().NotBeNullOrEmpty();
        result.Message.Should().Be("No se pudo actualizar la wallet");
        result.Data.Should().BeNull();
    }

    private static EffectTypeValue GetAction(WalletOperationValue action) =>
        action == WalletOperationValue.Credit ? EffectTypeValue.Credit : EffectTypeValue.Debit;
}
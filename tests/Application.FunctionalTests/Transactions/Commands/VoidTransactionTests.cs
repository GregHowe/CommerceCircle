using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Exceptions;
using N1coLoyalty.Application.FunctionalTests.Helpers;
using N1coLoyalty.Domain.Entities;
using N1coLoyalty.Application.Users.Commands;
using N1coLoyalty.Domain.Enums;
using N1coLoyalty.Application.Common.Constants;
using N1coLoyalty.Application.Transactions.Commands.VoidTransaction;

namespace N1coLoyalty.Application.FunctionalTests.Transactions.Commands;

using static Testing;
using static TransactionHelpers;
using static UserWalletBalanceHelpers;

public class VoidTransactionTests : BaseTestFixture
{
    private Mock<IWalletsService> _walletsService = new();
    private string _reason;
    private WalletActionValue _action;
    private WalletActionValue _actionVoid;
    private decimal _amount;
    private decimal _historicalCredit;
    private string _typeError = string.Empty;

    [SetUp]
    public async Task Setup()
    {
        _reason = "reason";
        _amount = 500;
        _historicalCredit = 200;

        // Create User
        await SendAsync(new CreateUserCommand());
        _walletsService = GetServiceMock<IWalletsService>();

        // Configure Revert
        _walletsService.Setup(x => x.Void(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto
            {
                Credit = _amount, Debit = 0, HistoricalCredit = _historicalCredit
            });
    }

    [Test]
    public async Task Should_ThrowException_When_Reason_Is_Empty()
    {
        //arrange
        _reason = string.Empty;
        var user = await FirstAsync<User>(x => true);
        var transaction = await CreateTransactionMock(user, _amount);
        _action = WalletActionValue.Create;
        await CreateUserWalletBalanceMock(transaction, user, _action);

        var command = new VoidTransactionCommand { TransactionId = transaction.Id, Reason = _reason };

        //action
        var exceptionAssertions = await FluentActions.Invoking(() =>
                SendAsync(command))
            .Should()
            .ThrowAsync<ValidationException>();

        ////assert
        _typeError = "Reason";
        var errors = exceptionAssertions.Which.Errors;
        errors.Should().HaveCountGreaterThan(0);
        errors.Should().ContainKey(_typeError);
        errors[_typeError].Should().Contain("La razón es requerida");
    }

    [Test]
    public async Task Should_Void_Transaction_Credit()
    {
        //arrange
        var user = await FirstAsync<User>(x => true);
        var transaction = await CreateTransactionMock(user, _amount);
        _action = WalletActionValue.Credit;
        _actionVoid = WalletActionValue.CreditVoid;
        await CreateUserWalletBalanceMock(transaction, user, _action);

        var command = new VoidTransactionCommand { TransactionId = transaction.Id, Reason = _reason };
        //action
        var result = await SendAsync(command);

        //assert
        result.Success.Should().BeTrue();

        var transactionVoid = await FirstAsync<Transaction>(x => x.TransactionType == EffectTypeValue.Revert);
        transactionVoid.Should().NotBeNull();
        transactionVoid.ProfileSessionId.Should().BeNull();
        transactionVoid.TransactionSubType.Should().Be(EffectSubTypeValue.Point);
        transactionVoid.TransactionType.Should().Be(EffectTypeValue.Revert);
        transactionVoid.UserId.Should().Be(user.Id);
        transactionVoid.Description.Should().Be(_reason);
        transactionVoid.Name.Should().Be(TransactionName.Revert);
        transactionVoid.TransactionStatus.Should().Be(TransactionStatusValue.Redeemed);
        transactionVoid.TransactionOrigin.Should().Be(TransactionOriginValue.Admin);
        transactionVoid.Amount.Should().Be(_amount);

        var userWalletBalance = await FirstAsync<UserWalletBalance>(x => x.Action == _actionVoid);
        userWalletBalance.Should().NotBeNull();
        userWalletBalance.Reason.Should().Be(TransactionName.Revert);
        userWalletBalance.UserId.Should().Be(user.Id);
        userWalletBalance.Action.Should().Be(_actionVoid);
        userWalletBalance.Amount.Should().Be(_amount);
        userWalletBalance.TransactionId.Should().Be(transactionVoid.Id);

        result.Code.Should().Be("OK");
        result.Message.Should().NotBeNullOrEmpty();
        result.Data.Should().BeNull();
    }

    [Test]
    public async Task Should_Void_Transaction_With_Session_Id()
    {
        //arrange
        var user = await FirstAsync<User>(x => true);
        var transaction = await CreateTransactionWithSessionIdMock(user, _amount);
        _action = WalletActionValue.Credit;
        _actionVoid = WalletActionValue.CreditVoid;
        await CreateUserWalletBalanceMock(transaction, user, _action);

        var command = new VoidTransactionCommand { TransactionId = transaction.Id, Reason = _reason };
        //action
        var result = await SendAsync(command);

        //assert
        result.Success.Should().BeTrue();

        var transactionVoid = await FirstAsync<Transaction>(x => x.TransactionType == EffectTypeValue.Revert);
        transactionVoid.ProfileSessionId.Should().NotBeNull();
        transactionVoid.Should().NotBeNull();
        transactionVoid.TransactionSubType.Should().Be(EffectSubTypeValue.Point);
        transactionVoid.TransactionType.Should().Be(EffectTypeValue.Revert);
        transactionVoid.UserId.Should().Be(user.Id);
        transactionVoid.Description.Should().Be(transaction.Description);
        transactionVoid.Name.Should().Be(TransactionName.Revert);
        transactionVoid.TransactionStatus.Should().Be(TransactionStatusValue.Redeemed);
        transactionVoid.TransactionOrigin.Should().Be(transaction.TransactionOrigin);
        transactionVoid.Amount.Should().Be(_amount);

        var userWalletBalance = await FirstAsync<UserWalletBalance>(x => x.Action == _actionVoid);
        userWalletBalance.Should().NotBeNull();
        userWalletBalance.Reason.Should().Be(TransactionName.Revert);
        userWalletBalance.UserId.Should().Be(user.Id);
        userWalletBalance.Action.Should().Be(_actionVoid);
        userWalletBalance.Amount.Should().Be(_amount);
        userWalletBalance.TransactionId.Should().Be(transactionVoid.Id);

        result.Code.Should().Be("OK");
        result.Message.Should().NotBeNullOrEmpty();
        result.Data.Should().BeNull();
    }

    [Test]
    public async Task Should_Void_Transaction_Debit()
    {
        //arrange
        var user = await FirstAsync<User>(x => true);
        var transaction = await CreateTransactionMock(user, _amount);
        _action = WalletActionValue.Debit;
        _actionVoid = WalletActionValue.DebitVoid;
        await CreateUserWalletBalanceMock(transaction, user, _action);

        var command = new VoidTransactionCommand { TransactionId = transaction.Id, Reason = _reason };
        //action
        var result = await SendAsync(command);

        //assert
        result.Success.Should().BeTrue();

        var transactionVoid = await FirstAsync<Transaction>(x => x.TransactionType == EffectTypeValue.Revert);
        
        transactionVoid.Should().NotBeNull();
        transactionVoid.TransactionSubType.Should().Be(EffectSubTypeValue.Point);
        transactionVoid.TransactionType.Should().Be(EffectTypeValue.Revert);
        transactionVoid.UserId.Should().Be(user.Id);
        transactionVoid.Description.Should().Be(_reason);
        transactionVoid.TransactionStatus.Should().Be(TransactionStatusValue.Redeemed);
        transactionVoid.TransactionOrigin.Should().Be(TransactionOriginValue.Admin);
        transactionVoid.Amount.Should().Be(_amount);

        var userWalletBalance = await FirstAsync<UserWalletBalance>(x => x.Action == _actionVoid);
        userWalletBalance.Should().NotBeNull();
        userWalletBalance.Reason.Should().Be(TransactionName.Revert);
        userWalletBalance.UserId.Should().Be(user.Id);
        userWalletBalance.Action.Should().Be(_actionVoid);
        userWalletBalance.Amount.Should().Be(_amount);
        userWalletBalance.TransactionId.Should().Be(transactionVoid.Id);

        result.Code.Should().Be("OK");
        result.Message.Should().NotBeNullOrEmpty();
        result.Data.Should().BeNull();
    }

    [Test]
    public async Task Should_ThrowException_When_IdTransaction_Is_Empty()
    {
        //arrange
        var command = new VoidTransactionCommand { TransactionId = Guid.Empty, Reason = _reason };

        //action
        var exceptionAssertions = await FluentActions.Invoking(() =>
                SendAsync(command))
            .Should()
            .ThrowAsync<ValidationException>();

        _typeError = "TransactionId";

        //assert
        var errors = exceptionAssertions.Which.Errors;
        errors.Should().HaveCountGreaterThan(0);
        errors.Should().ContainKey(_typeError);
        errors[_typeError].Should().Contain("El TransactionId es requerido");
        
        var transactionVoid = await FirstOrDefaultAsync<Transaction>(x=>x.TransactionType == EffectTypeValue.Revert);
        transactionVoid.Should().BeNull();
        var userWalletBalance = await FirstOrDefaultAsync<UserWalletBalance>(x=>x.Action == _actionVoid);
        userWalletBalance.Should().BeNull();
        
    }

    [Test]
    public async Task Should_ThrowException_When_No_Exists_WalletBalance()
    {
        //arrange   
        var user = await FirstAsync<User>(x => true);
        var transaction = await CreateTransactionMock(user, _amount);

        var command = new VoidTransactionCommand { TransactionId = transaction.Id, Reason = _reason };

        //action
        var exceptionAssertions = await FluentActions.Invoking(() =>
                SendAsync(command))
            .Should()
            .ThrowAsync<ValidationException>();

        //assert
        _typeError = string.Empty;
        var errors = exceptionAssertions.Which.Errors;
        errors.Should().HaveCountGreaterThan(0);
        errors.Should().ContainKey(_typeError);
        errors[_typeError].Should().Contain("No hay movimiento de Wallet asociado a la transacción");
        
        var transactionVoid = await FirstOrDefaultAsync<Transaction>(x=>x.TransactionType == EffectTypeValue.Revert);
        transactionVoid.Should().BeNull();
        var userWalletBalance = await FirstOrDefaultAsync<UserWalletBalance>(x=>x.Action == _actionVoid);
        userWalletBalance.Should().BeNull();
    }

    [Test]
    public async Task Should_ThrowException_When_ActionWalletBalance_Is_Not_Credit_Or_Debit()
    {
        //arrange   
        var user = await FirstAsync<User>(x => true);
        var transaction = await CreateTransactionMock(user, _amount);
        _action = WalletActionValue.Create;
        _actionVoid = WalletActionValue.CreditVoid;
        await CreateUserWalletBalanceMock(transaction, user, _action);

        var command = new VoidTransactionCommand { TransactionId = transaction.Id, Reason = _reason };

        //action
        var exceptionAssertions = await FluentActions.Invoking(() =>
                SendAsync(command))
            .Should()
            .ThrowAsync<ValidationException>();

        //assert
        _typeError = string.Empty;
        var errors = exceptionAssertions.Which.Errors;
        errors.Should().HaveCountGreaterThan(0);
        errors.Should().ContainKey(_typeError);
        errors[_typeError].Should().Contain("Solo se pueden anular transacciones de Crédito o Débito");
        
        var transactionVoid = await FirstOrDefaultAsync<Transaction>(x=>x.TransactionType == EffectTypeValue.Revert);
        transactionVoid.Should().BeNull();
        var userWalletBalance = await FirstOrDefaultAsync<UserWalletBalance>(x=>x.Action == _actionVoid);
        userWalletBalance.Should().BeNull();
    }

    [Test]
    public async Task Should_Return_Error_When_WalletResponseIsNull()
    {
        //arrange
        _walletsService.Setup(x => x.Void(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(() => null);

        var user = await FirstAsync<User>(x => true);
        var transaction = await CreateTransactionMock(user, _amount);
        _action = WalletActionValue.Credit;
        await CreateUserWalletBalanceMock(transaction, user, _action);

        var command = new VoidTransactionCommand { TransactionId = transaction.Id, Reason = _reason };

        //action
        var result = await SendAsync(command);

        //assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("ERROR");
        result.Message.Should().NotBeNullOrEmpty();
        result.Message.Should().Be("El proceso de anulación ha fallado");
        result.Data.Should().BeNull();
    }

    [Test]
    public async Task Should_Return_Error_When_Transaction_Doesnt_Exists()
    {
        _actionVoid = WalletActionValue.CreditVoid;
        var command = new VoidTransactionCommand { TransactionId = Guid.NewGuid(), Reason = _reason };

        // Assert
        var exceptionAssertions = await FluentActions.Invoking(() =>
            SendAsync(command)).Should().ThrowAsync<ValidationException>();

        var errors = exceptionAssertions.Which.Errors;
        errors.Should().ContainKey("TransactionId");
        errors["TransactionId"].Should().Contain("La transacción no existe");
        
        var transactionVoid = await FirstOrDefaultAsync<Transaction>(x=>x.TransactionType == EffectTypeValue.Revert);
        transactionVoid.Should().BeNull();
        var userWalletBalance = await FirstOrDefaultAsync<UserWalletBalance>(x=>x.Action == _actionVoid);
        userWalletBalance.Should().BeNull();
    }
}

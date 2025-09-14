using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Domain.Enums;

namespace N1coLoyalty.Application.Transactions.Commands.VoidTransaction;

public class VoidTransactionCommandValidator : AbstractValidator<VoidTransactionCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public VoidTransactionCommandValidator(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Reason)
        .NotEmpty()
        .WithMessage("La razón es requerida");

        RuleFor(x => x.TransactionId)
        .NotEqual(Guid.Empty)
        .NotNull()
        .NotEmpty()
        .WithMessage("El TransactionId es requerido");

        RuleFor(x => x.TransactionId)
        .MustAsync(TransactionExists)
        .WithMessage("La transacción no existe");

        RuleFor(x => x)
        .MustAsync(TransactionHasUserAWalletBalance)
        .WithMessage("No hay movimiento de Wallet asociado a la transacción");

        RuleFor(x => x)
        .MustAsync(ActionAllowed)
        .WithMessage("Solo se pueden anular transacciones de Crédito o Débito");
    }

    private async Task<bool> TransactionExists(Guid transactionId, CancellationToken cancellationToken)
    {
        var transactionQueryable = from t in _dbContext.Transactions
                                   where t.Id == transactionId
                                   select t;
        var transaction = await transactionQueryable.FirstOrDefaultAsync(cancellationToken: cancellationToken);
        return transaction != null;
    }

    private async Task<bool> TransactionHasUserAWalletBalance(VoidTransactionCommand command, CancellationToken cancellationToken)
    {
        var userWalletBalanceQueryable = from t in _dbContext.Transactions
                                         join uwb in _dbContext.UserWalletBalances on t.Id equals uwb.TransactionId
                                         where t.Id == command.TransactionId
                                         select uwb;

        var userWalletBalance = await userWalletBalanceQueryable.FirstOrDefaultAsync(cancellationToken);
        return userWalletBalance != null;
    }

    private async Task<bool> ActionAllowed(VoidTransactionCommand command, CancellationToken cancellationToken)
    {
        var userWalletBalanceQueryable = from t in _dbContext.Transactions
                                         join uwb in _dbContext.UserWalletBalances on t.Id equals uwb.TransactionId
                                         where t.Id == command.TransactionId
                                         select uwb;

        var userWalletBalance = await userWalletBalanceQueryable.FirstOrDefaultAsync(cancellationToken);
        var action = userWalletBalance?.Action;
        return action is WalletActionValue.Credit or WalletActionValue.Debit;
    }
}

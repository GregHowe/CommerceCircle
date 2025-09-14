namespace N1coLoyalty.Application.UserWalletBalances.Commands.UpdateUserWalletBalance;

public class UpdateUserWalletBalanceCommandValidator : AbstractValidator<UpdateUserWalletBalanceCommand>
{
    public UpdateUserWalletBalanceCommandValidator()
    {
        RuleFor(x => x.UserPhone)
            .NotEmpty()
            .WithMessage("El número del usuario es requerido");

        RuleFor(x => x.Operation)
            .IsInEnum()
            .WithMessage("Operación no válida");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("La razón es requerida");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("El monto debe ser mayor a 0");
    }
}
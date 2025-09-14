using N1coLoyalty.Application.Common.Behaviours;
using N1coLoyalty.Application.Common.Exceptions;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Security;
using N1coLoyalty.Application.Transactions.Queries.GetAdminTransactions;
using NSubstitute;
using NUnit.Framework;

namespace N1coLoyalty.Application.UnitTests.Common.Behaviours;

public class AuthorizationTests
{
    private readonly IUser _currentUserService = Substitute.For<IUser>();

    [Test]
    public void ShouldThrowAuthorizationException()
    {
        _currentUserService.Id.Returns("admin.user@email.com");
        _currentUserService.Permissions.Returns(System.Array.Empty<string>());

        var authorizationBehaviour =
            new AuthorizationBehaviour<GetAdminTransactionsQuery, TransactionsAllVm>(_currentUserService);

        Assert.ThrowsAsync<AuthorizationException>(() => authorizationBehaviour.Handle(new GetAdminTransactionsQuery(),
            () => Task.FromResult(new TransactionsAllVm()), new CancellationToken()));
    }

    [Test]
    public async Task ShouldNotThrowAuthorizationException()
    {
        _currentUserService.Id.Returns("admin.user@email.com");
        _currentUserService.Permissions.Returns(new[] { Permission.ReadTransactions });

        var authorizationBehaviour =
            new AuthorizationBehaviour<GetAdminTransactionsQuery, TransactionsAllVm>(_currentUserService);

        await authorizationBehaviour.Handle(new GetAdminTransactionsQuery(),
            () => Task.FromResult(new TransactionsAllVm()), new CancellationToken());

        Assert.Pass();
    }
}
using System.Data.Common;

namespace N1coLoyalty.Application.FunctionalTests;

public interface ITestDatabase
{
    Task InitialiseAsync();

    DbConnection GetConnection();

    Task ResetAsync();

    Task DisposeAsync();
}

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace N1coLoyalty.Infrastructure.Data;

public static class InitializerExtensions
{
    public static async Task InitialiseDatabaseAsync(this IHost app)
    {
        using var scope = app.Services.CreateScope();

        var initializer = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitializer>();

        await initializer.InitialiseAsync();

        await initializer.SeedAsync();
    }
    
}
using N1coLoyalty.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace N1coLoyalty.Infrastructure.Data;

public class ApplicationDbContextInitializer(
    ILogger<ApplicationDbContextInitializer> logger,
    ApplicationDbContext context)
{
    public async Task InitialiseAsync()
    {
        try
        {
            await context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initialising the database");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private async Task TrySeedAsync()
    {
        if (!await context.TermsConditionsInfo.AnyAsync())
        {
            await context.AddAsync(new TermsConditionsInfo()
            {
                Id = Guid.NewGuid(),
                Version = "1",
                Url = "https://n1co.com/terminos-y-condiciones/",
                IsCurrent = true
            });
        }
        
        await context.SaveChangesAsync();
    }
}

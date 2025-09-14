using System.Linq.Expressions;
using MassTransit.Testing;
using N1coLoyalty.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace N1coLoyalty.Application.FunctionalTests;

[SetUpFixture]
public class Testing
{
    internal const string ReferralCode = "REFERRAL-CODE";
    private static ITestDatabase? _database;
    private static CustomWebApplicationFactory _factory = null!;
    private static IServiceScopeFactory _scopeFactory = null!;
    private static string? _userId;
    internal static ITestHarness Harness = null!;

    [OneTimeSetUp]
    public async Task RunBeforeAnyTests()
    {
        _database = await TestDatabaseFactory.CreateAsync();

        _factory = new CustomWebApplicationFactory(_database.GetConnection());
        
        _scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
        Harness = _factory.Services.GetRequiredService<ITestHarness>(); 
        await Harness.Start();
    }

    public static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        var scope = _scopeFactory.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        return await mediator.Send(request);
    }

    public static string? GetUserId()
    {
        return _userId;
    }

    public static async Task ResetState()
    {
        try
        {
            await _database!.ResetAsync();
        }
        catch (Exception)
        {
            // ignored
        }

        _userId = null;
    }

    public static async Task<TEntity?> FindAsync<TEntity>(params object[] keyValues)
        where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.FindAsync<TEntity>(keyValues);
    }
    
    public static async Task<T> FirstAsync<T>(Expression<Func<T, bool>> predicate)
        where T : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Set<T>().FirstAsync(predicate);
    }
    
    public static async Task<T?> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate)
        where T : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Set<T>().FirstOrDefaultAsync(predicate);
    }
    

    public static async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Add(entity);

        await context.SaveChangesAsync();
    }
    
    public static async Task AttachEntity<TEntity>(TEntity entity)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (entity != null)
        {
            context.Attach(entity);
        }

        await context.SaveChangesAsync();
    }
    
    public static async Task UpdateAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Update(entity);

        await context.SaveChangesAsync();
    }

    public static async Task RemoveAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Remove(entity);

        await context.SaveChangesAsync();
    }

    public static async Task<IList<T>> ToListAsync<T>(Expression<Func<T, bool>> predicate)
        where T : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetService<ApplicationDbContext>();

        if (context is null)
        {
            return new List<T>();
        }

        return await context.Set<T>().Where(predicate).ToListAsync();

    }
    
    public static async Task<IList<T>> AllToListAsync<T>()
        where T : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetService<ApplicationDbContext>();

        if (context is null)
        {
            return new List<T>();
        }

        return await context.Set<T>().ToListAsync();

    }

    public static async Task<int> CountAsync<TEntity>() where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Set<TEntity>().CountAsync();
    }

    private static TService GetService<TService>()
    {
        using var scope = _scopeFactory.CreateScope();
        return scope.ServiceProvider.GetService<TService>() ?? throw new InvalidOperationException($"Cannot get service {typeof(TService)}");
    }

    internal static Mock<TService> GetServiceMock<TService>()
        where TService : class
    {
        return Mock.Get(GetService<TService>());
    }

    [OneTimeTearDown]
    public async Task RunAfterAnyTests()
    {
        await Harness.Stop();
        await _database!.DisposeAsync();
        await _factory.DisposeAsync();
    }
}

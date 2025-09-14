using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Repositories;
using N1coLoyalty.Application.Common.Services;
using N1coLoyalty.Application.IntegrationEvents;
using N1coLoyalty.Application.IssuingEvents;
using N1coLoyalty.Application.NotificationEvents;
using N1coLoyalty.Infrastructure.Data;
using N1coLoyalty.Infrastructure.Data.Interceptors;
using N1coLoyalty.Infrastructure.Data.Repositories;
using N1coLoyalty.Infrastructure.HttpClients.CashBack;
using N1coLoyalty.Infrastructure.HttpClients.CashBack.Handlers;
using N1coLoyalty.Infrastructure.HttpClients.LoyaltyCore;
using N1coLoyalty.Infrastructure.HttpClients.LoyaltyCore.Handlers;
using N1coLoyalty.Infrastructure.Identity;
using N1coLoyalty.Infrastructure.IntegrationEvents;
using N1coLoyalty.Infrastructure.IssuingEvents;
using N1coLoyalty.Infrastructure.NotificationEvents;
using N1coLoyalty.Infrastructure.Services;
using N1coLoyalty.Infrastructure.Services.CashBack;
using N1coLoyalty.Infrastructure.Services.LoyaltyEngine;
using N1coLoyalty.Infrastructure.Services.Wallets;

namespace N1coLoyalty.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        Guard.Against.Null(connectionString, message: "Connection string 'DefaultConnection' not found.");

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());

            options.UseSqlServer(connectionString);
        });
        
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<ApplicationDbContextInitializer>();
        services.AddAuthorizationBuilder();

        services.AddSingleton(TimeProvider.System);
        services.AddTransient<IDateTime, DateTimeService>();
        services.AddTransient<IIdentityService, IdentityService>();
        services.AddTransient<ILoyaltyEngine, LoyaltyEngineService>();
        services.AddTransient<ITransactionRepository, TransactionRepository>();
        services.AddTransient<ITermsConditionsRepository, TermsConditionsRepository>();
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<ITermsConditionsRepository, TermsConditionsRepository>();
        services.AddTransient<ITermsConditionsAcceptanceRepository, TermsConditionsAcceptanceRepository>();
        services.AddTransient<ILoyaltyEventService, LoyaltyEventService>();
        services.AddTransient<ICashBackService, CashBackService>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.Authority = configuration["AuthenticationService:Authority"];
            options.Audience = configuration["AuthenticationService:Audience"];
        });
        
        services.AddAuthorization();
        
        services.AddScoped<LoyaltyCoreHttpClientDelegatingHandler>();
        services.AddHttpClient<LoyaltyCoreHttpClient>()
            .AddHttpMessageHandler<LoyaltyCoreHttpClientDelegatingHandler>()
            .ConfigureHttpClient((_, httpClient) =>
            {
                httpClient.BaseAddress = new Uri(configuration["LoyaltyCore:Service:Url"] ?? string.Empty);
                httpClient.Timeout = TimeSpan.FromMinutes(3);
            });

        services.AddScoped<CashBackHttpClientDelegatingHandler>();
        services.AddHttpClient<CashBackHttpClient>()
            .AddHttpMessageHandler<CashBackHttpClientDelegatingHandler>()
            .ConfigureHttpClient((_, httpClient) =>
            {
                httpClient.BaseAddress = new Uri(configuration["CashBack:Service:Url"] ?? string.Empty);
                httpClient.Timeout = TimeSpan.FromMinutes(3);
            });
        
        services.AddScoped<IWalletsService, WalletsService>();
        
        AddMainMassTransit(services, configuration);
        AddIssuingMassTransit(services, configuration);
        AddNotificationMassTransit(services, configuration);
        services.AddScoped<IIntegrationEventsBus, IntegrationEventsBus>();
        services.AddScoped<INotificationEventBus, NotificationEventsBus>();
        services.AddScoped<IIssuingEventsBus, IssuingEventsBus>();

        return services;
    }
    
    private static void AddMainMassTransit(IServiceCollection services, IConfiguration configuration)
    {
        var appName = configuration.GetValue("ApplicationName", "Api");
        if (appName == "EventWorker")
        {
            return;
        }
        
        var connectionString = configuration.GetConnectionString("AzureServiceBus");
        if (string.IsNullOrEmpty(connectionString))
        {
            return;
        }

        services.AddMassTransit(configurator =>
        {
            configurator.UsingAzureServiceBus((context, cfg) =>
            {
                cfg.Host(connectionString);
                cfg.ConfigureEndpoints(context);
            });
        });
    }
    
    private static void AddNotificationMassTransit(IServiceCollection services, IConfiguration configuration)
    {
        var notificationConnectionString = configuration.GetConnectionString("NotificationAzureServiceBus");
        if (string.IsNullOrEmpty(notificationConnectionString))
        {
            return;
        }
        
        services.AddMassTransit<INotificationBus>(configurator =>
        {
            configurator.UsingAzureServiceBus((context, cfg) =>
            {
                cfg.Host(notificationConnectionString);
                cfg.ConfigureEndpoints(context);
            });
        });
    }
    
    private static void AddIssuingMassTransit(IServiceCollection services, IConfiguration configuration)
    {
        var issuingConnectionString = configuration.GetConnectionString("IssuingAzureServiceBus");
        if (string.IsNullOrEmpty(issuingConnectionString))
        {
            return;
        }
        var appName = configuration.GetValue("ApplicationName", "Api");
        if (appName == "EventWorker")
        {
            return;
        }
        
        services.AddMassTransit<IIssuingBus>(configurator =>
        {
            configurator.UsingAzureServiceBus((context, cfg) =>
            {
                cfg.Host(issuingConnectionString);
                cfg.ConfigureEndpoints(context);
            });
        });
    }
}

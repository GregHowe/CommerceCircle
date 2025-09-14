using System.Data.Common;
using System.Reflection;
using MassTransit;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using N1coLoyalty.Application.Common.Services;
using N1coLoyalty.Application.FunctionalTests.ExtensionMethods;
using N1coLoyalty.Application.IntegrationEvents;
using N1coLoyalty.Application.IssuingEvents;
using N1coLoyalty.Application.NotificationEvents;

namespace N1coLoyalty.Application.FunctionalTests;

public class CustomWebApplicationFactory(DbConnection connection) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services
                .RemoveAll<DbContextOptions<ApplicationDbContext>>()
                .AddDbContext<ApplicationDbContext>((sp, options) =>
                {
                    options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                    options.UseSqlServer(connection);
                });
            
            // add scoped for IApplicationDbContext
            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

            // add IWalletsService mocks
            var walletService = new Mock<IWalletsService>();
            services.SwapTransient(_ => walletService.Object);
            
            // add IUser mocks
            var userService = new Mock<IUser>();
            services.SwapTransient(_ => userService.Object);
            
            // add IDateTime mocks
            var dateTime = new Mock<IDateTime>();
            services.SwapTransient(_ => dateTime.Object);
            
            // add MassTransitTestHarness
            services.AddMassTransitTestHarness(cfg =>
            {
                var applicationAssembly = Assembly.GetAssembly(typeof(DependencyInjection));
                cfg.AddConsumers(applicationAssembly);
                cfg.AddSagas(applicationAssembly);
                cfg.AddActivities(applicationAssembly);
            });
            
            //add IIntegrationEventsBus
            var integrationEventsBus = new Mock<IIntegrationEventsBus>();
            services.SwapTransient(_ => integrationEventsBus.Object);
            
            // add Loyalty Engine Service
            var loyaltyEngineService = new Mock<ILoyaltyEngine>();
            services.SwapTransient(_ => loyaltyEngineService.Object);
            
            // add Cashback Service
            var cashbackService = new Mock<ICashBackService>();
            services.SwapTransient(_ => cashbackService.Object);
            
            // add INotificationEventBus
            var notificationEventBus = new Mock<INotificationEventBus>();
            services.SwapTransient(_=> notificationEventBus.Object);
            
            // add IIssuingEventBus
            var issuingEventBus = new Mock<IIssuingEventsBus>();
            services.SwapTransient(_=> issuingEventBus.Object);
        });
    }
}

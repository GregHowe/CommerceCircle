using MassTransit;
using System.Reflection;
using BusinessEvents.Contracts.BillPayments;
using BusinessEvents.Contracts.Issuing;
using BusinessEvents.Contracts.Loyalty;
using BusinessEvents.Contracts.Loyalty.Models;
using N1coLoyalty.Application;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Consumers.BillPayment;
using N1coLoyalty.Application.Consumers.Event;
using N1coLoyalty.Application.Consumers.Transaction;
using N1coLoyalty.EventWorker.Services;
using N1coLoyalty.Infrastructure;
using N1coLoyalty.Application.Consumers.Referral;
using N1coLoyalty.Application.Consumers.Session;
using N1coLoyalty.Application.Consumers.Wallet;
using Serilog;
using Serilog.Formatting.Compact;
using N1coLoyalty.Infrastructure.Data;

namespace N1coLoyalty.EventWorker;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = CreateHostBuilder(args);
        var app = builder.Build();
        await app.InitialiseDatabaseAsync();
        await app.RunAsync();
    }   

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
         .ConfigureServices((hostContext, services) =>
         {
             services.AddScoped<IUser, CurrentUser>();
             services.AddHttpContextAccessor();

             services.AddRouting();

             services.AddApplicationServices();
             services.AddInfrastructureServices(hostContext.Configuration);

             services.AddMassTransit(x =>
             {
                 var applicationAssembly = Assembly.GetAssembly(typeof(Application.DependencyInjection));
                 x.AddConsumers(applicationAssembly);

                 x.UsingAzureServiceBus((context, cfg) =>
                 {
                     var connectionString = hostContext.Configuration.GetConnectionString("AzureServiceBus");
                     cfg.Host(connectionString);
                     ConfigureMainEndpoints(cfg, context);
                 });
             });

             services.AddMassTransit<Application.IssuingEvents.IIssuingBus>(x =>
             {
                 var applicationAssembly = Assembly.GetAssembly(typeof(Application.DependencyInjection));
                 x.AddConsumers(applicationAssembly);

                 x.UsingAzureServiceBus((context, cfg) =>
                 {
                     var connectionString = hostContext.Configuration.GetConnectionString("IssuingAzureServiceBus");
                     cfg.Host(connectionString);
                     ConfigureIssuingEndpoints(cfg, context);
                 });
             });
             services.AddMassTransit<IBillPaymentBus>(x =>
             {
                 var applicationAssembly = Assembly.GetAssembly(typeof(Application.DependencyInjection));
                 x.AddConsumers(applicationAssembly);

                 x.UsingAzureServiceBus((context, cfg) =>
                 {
                     var connectionString = hostContext.Configuration.GetConnectionString("BillPaymentsAzureServiceBus");
                     cfg.Host(connectionString);
                     ConfigureBillPaymentsEndpoints(cfg, context);
                 });
             });
         })
         .ConfigureAppConfiguration((_, config) =>
         {
             config.AddEnvironmentVariables();
         })
        .UseSerilog((context, configuration) =>
        {
            configuration.Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentUserName()
                .WriteTo.Console(new RenderedCompactJsonFormatter())
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);
        });

    private static void ConfigureMainEndpoints(IServiceBusBusFactoryConfigurator cfg, IBusRegistrationContext context)
    {
        cfg.ReceiveEndpoint(nameof(EventProcessed), e => { e.ConfigureConsumer<EventProcessedConsumer>(context); });
        cfg.ReceiveEndpoint(nameof(WalletTransaction), e => { e.ConfigureConsumer<WalletTransactionConsumer>(context); });
        cfg.ReceiveEndpoint(nameof(ProfileSessionEffectsVoided), e => { e.ConfigureConsumer<ProfileSessionEffectsVoidedConsumer>(context); });
    }

    private static void ConfigureIssuingEndpoints(IServiceBusBusFactoryConfigurator cfg, IBusRegistrationContext context)
    {
        cfg.ReceiveEndpoint(nameof(TransactionStatusChange), e => { e.ConfigureConsumer<TransactionStatusChangeConsumer>(context); });
        cfg.ReceiveEndpoint(nameof(ReferralCodeRedeemed), e => { e.ConfigureConsumer<ReferralCodeRedeemedConsumer>(context); });
    }

    private static void ConfigureBillPaymentsEndpoints(IServiceBusBusFactoryConfigurator cfg, IBusRegistrationContext context)
    {
        cfg.ReceiveEndpoint(nameof(BillPaid), e => { e.ConfigureConsumer<BillPaidConsumer>(context); });
    }
}
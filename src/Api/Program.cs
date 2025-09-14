using HotChocolate.Types.Pagination;
using N1coLoyalty.Api;
using N1coLoyalty.Api.GraphQL.Mutations;
using N1coLoyalty.Api.GraphQL.ObjectTypes;
using N1coLoyalty.Api.GraphQL.Queries;
using N1coLoyalty.Application;
using N1coLoyalty.Infrastructure;
using N1coLoyalty.Infrastructure.Data;
using Serilog;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

var logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentUserName()
    .Enrich.WithProperty("environment", builder.Environment.EnvironmentName)
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Host.UseSerilog(logger);

// Add services to the container.
builder.Services.AddKeyVaultIfConfigured(builder.Configuration);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddWebServices(builder.Configuration);

builder.Services.AddGraphQLServer()
    .AllowIntrospection(builder.Environment.IsDevelopment())
    .AddAuthorization()
    .ConfigureSchema(sb => sb.ModifyOptions(opts => opts.StrictValidation = false))
    .SetPagingOptions(new PagingOptions { IncludeTotalCount = true })
    .AddQueryType(d => d.Name("Query"))
    .AddType<BalanceQueries>()
    .AddType<RewardsQueries>()
    .AddType<GameQueries>()
    .AddType<TransactionQueries>()
    .AddType<TermsConditionsQueries>()
    .AddType<ProfileQueries>()
    .AddType<UserQueries>()
    .AddType<GameSettingsDtoType>()
    .AddMutationType(d => d.Name("Mutations"))
    .AddType<UserMutations>()
    .AddType<EventMutations>()
    .AddType<TermsConditionsAcceptanceMutations>()
    .AddFiltering()
    .AddSorting()
    .AddProjections();

var app = builder.Build();

await app.InitialiseDatabaseAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsProduction())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHealthChecks("/health");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSwaggerUi(settings =>
{
    settings.Path = "/api";
    settings.DocumentPath = "/api/specification.json";
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapRazorPages();

app.MapFallbackToFile("index.html");

app.UseExceptionHandler(_ => { });

app.Map("/", () => Results.Redirect("/api"));

app.MapEndpoints();
app.MapGraphQL();

app.Run();

public partial class Program { }

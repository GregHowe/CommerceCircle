using Microsoft.AspNetCore.Mvc.Filters;

namespace N1coLoyalty.AdminApi.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
{
    private static string ApiKeyHeader => "x-api-key";
    private static string ApiKey => "AdminService:ApiKey";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeader, out var apiKeyVal))
        {
            context.HttpContext.Response.StatusCode = 401;
            await context.HttpContext.Response.WriteAsync("Api Key was not provided");
            return;
        }

        var appSettings = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var apiKey = appSettings.GetValue<string>(ApiKey);
        if (!apiKey.Equals(apiKeyVal))
        {
            context.HttpContext.Response.StatusCode = 401;
            await context.HttpContext.Response.WriteAsync("Unauthorized client");
            return;
        }
        
        await next();
    }
}
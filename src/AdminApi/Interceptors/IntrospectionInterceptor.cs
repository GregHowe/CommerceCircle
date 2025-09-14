using HotChocolate.AspNetCore;
using HotChocolate.Execution;

namespace N1coLoyalty.AdminApi.Interceptors;

public class IntrospectionInterceptor(IWebHostEnvironment environment, IConfiguration configuration)
    : DefaultHttpRequestInterceptor
{
    private readonly string? _introspectionKey = configuration["Graphql:IntrospectionKey"];

    public override ValueTask OnCreateAsync(HttpContext context,
        IRequestExecutor requestExecutor, IQueryRequestBuilder requestBuilder,
        CancellationToken cancellationToken)
    {
        if (environment.IsProduction())
        {
            if (context.Request.Headers.TryGetValue("x-introspection-key", out var headerValue) &&
                headerValue == _introspectionKey)
            {
                requestBuilder.AllowIntrospection();
            }
            else
            {
                requestBuilder.SetIntrospectionNotAllowedMessage("Introspection not allowed without valid key.");
            }
        }
        else
        {
            requestBuilder.AllowIntrospection(); // Allow introspection in development
        }

        return base.OnCreateAsync(context, requestExecutor, requestBuilder, cancellationToken);
    }
}
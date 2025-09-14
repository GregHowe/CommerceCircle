using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;

namespace N1coLoyalty.Infrastructure.HttpClients.CashBack.Handlers;

public class CashBackHttpClientDelegatingHandler(IConfiguration configuration): DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("ApiKey",configuration["CashBack:Service:ApiKey"]);
        
        return await base.SendAsync(request, cancellationToken);
    }
}

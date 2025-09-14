using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using N1coLoyalty.Infrastructure.HttpClients.CashBack.Models;

namespace N1coLoyalty.Infrastructure.HttpClients.CashBack;

public class CashBackHttpClient(HttpClient httpClient, ILogger<CashBackHttpClient> logger)
{
    public async Task<ApplyCashBackResponse> ApplyCashBackAsync(string phoneNumber, decimal amount, string reason, string description)
    {
        var servicePath = "/cashback/apply-cashback-loyalty";
        // request
        var request = SetApplyCashBackRequest(phoneNumber, amount, reason, description);
        var jsonString = JsonSerializer.Serialize(request);
        logger.LogInformation("Requested Issuing ApplyCashBack sent. Payload: {@Payload}", jsonString);

        // call api
        logger.LogInformation("Sending Issuing ApplyCashBack call. Url: {@BaseUrl}{@Url} ", httpClient.BaseAddress?.AbsoluteUri, servicePath);
        
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(servicePath, content);
        ApplyCashBackResponse applyCashBackResponse = new()
        {
            Success = false,
            Message = "Ha ocurrido un error",
            Code = "Error"
        };
        if (response.IsSuccessStatusCode)
        {
            applyCashBackResponse = await SetResponseSuccess(response, "Receiving Issuing ApplyCashBack response. Payload: {@Payload}") ?? applyCashBackResponse;
        }
        else
        {
            var contentResponse = await response.Content.ReadAsStringAsync();
            logger.LogError("Receiving Issuing ApplyCashBack response. HTTP Error {@HttpError} . Payload: {@Payload}", response.StatusCode, contentResponse);
        }

        return applyCashBackResponse;
    }
    
    private static ApplyCashBackRequest SetApplyCashBackRequest(string phoneNumber, decimal amount, string reason, string description)
    {
        return new ApplyCashBackRequest
        {
            PhoneNumber = phoneNumber,
            AmountTrx = amount,
            Reason = reason,
            Description = description,
            OriginType = "loyalty"
        };
    }
    
    private async Task<ApplyCashBackResponse?> SetResponseSuccess(HttpResponseMessage response, string info)
    {
        var jsonString = await response.Content.ReadAsStringAsync();
        logger.LogInformation(info, jsonString);
        return await response.Content.ReadFromJsonAsync<ApplyCashBackResponse>();
    }
}

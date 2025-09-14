using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using N1coLoyalty.Infrastructure.HttpClients.LoyaltyCore.Models;
using N1coLoyalty.Infrastructure.Services.Wallets;

namespace N1coLoyalty.Infrastructure.HttpClients.LoyaltyCore;

public class LoyaltyCoreHttpClient(HttpClient httpClient, ILogger<LoyaltyCoreHttpClient> logger, IConfiguration configuration)
{
    private readonly string? _loyaltyProgramId = configuration["LoyaltyCore:LoyaltyProgramIntegrationId"];

    public async Task<WalletBalanceResponse?> GetBalance(string profileIntegrationId)
    {
        // config and token
        var servicePath = $"/api/v1/loyalty_programs/{_loyaltyProgramId}/wallets/balance/{profileIntegrationId}";

        // call api
        logger.LogInformation("Sending Wallets GetBalance call. Url: {@BaseUrl}{@Url} ",
            httpClient.BaseAddress?.AbsoluteUri, servicePath);

        WalletBalanceResponse? walletBalance = null;
        try
        {
            walletBalance = await httpClient.GetFromJsonAsync<WalletBalanceResponse>(servicePath);
            var jsonString = JsonSerializer.Serialize(walletBalance);
            logger.LogInformation("Receiving Wallets GetBalance response. Payload: {@Payload}", jsonString);
            return walletBalance;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex,
                message: "Receiving Wallets GetBalance response. HTTP Error {@HttpError} . ErrorMessage: {@ErrorMessage} . StackTrace: {@StackTrace}",
                ex.StatusCode, ex.Message, ex.StackTrace);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Receiving Wallets GetBalance response. ErrorMessage: {@ErrorMessage} . StackTrace: {@StackTrace}",
                ex.Message, ex.StackTrace);
        }

        return walletBalance;
    }

    public async Task<WalletBalanceResponse?> Create(string profileIntegrationId)
    {
        // config and token
        var servicePath = $"/api/v1/loyalty_programs/{_loyaltyProgramId}/wallets/credit";

        // request
        var request = SetWalletTransactionRequest(profileIntegrationId, 0);

        var jsonString = JsonSerializer.Serialize(request);
        logger.LogInformation("Requested Wallets CreateWallet sent. Payload: {@Payload}", jsonString);

        // call api
        logger.LogInformation("Sending Wallets CreateWallet call. Url: {@BaseUrl}{@Url} ", httpClient.BaseAddress?.AbsoluteUri, servicePath);

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(servicePath, content);

        WalletBalanceResponse? walletBalanceTransaction = null;
        if (response.IsSuccessStatusCode)
        {
            walletBalanceTransaction = await SetResponseSuccess(response, "Receiving Wallets CreateWallet response. Payload: {@Payload}");
        }
        else
        {
            var contentResponse = await response.Content.ReadAsStringAsync();
            logger.LogError("Receiving Wallets CreateWallet response. HTTP Error {@HttpError} . Payload: {@Payload}", response.StatusCode, contentResponse);
        }

        return walletBalanceTransaction;
    }

    public async Task<WalletBalanceResponse?> Credit(string profileIntegrationId, decimal amount)
    {
        // config and token
        var servicePath = $"/api/v1/loyalty_programs/{_loyaltyProgramId}/wallets/credit";

        // request
        var request = SetWalletTransactionRequest(profileIntegrationId, amount);

        var jsonString = JsonSerializer.Serialize(request);
        logger.LogInformation("Requested Wallets Credit sent. Payload: {@Payload}", jsonString);

        // call api
        logger.LogInformation("Sending Wallets Credit call. Url: {@BaseUrl}{@Url} ", httpClient.BaseAddress?.AbsoluteUri, servicePath);
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(servicePath, content);

        WalletBalanceResponse? walletBalanceTransaction = null;
        if (response.IsSuccessStatusCode)
        {
            walletBalanceTransaction = await SetResponseSuccess(response, "Receiving Wallets Credit response. Payload: {@Payload}");
        }
        else
        {
            var contentResponse = await response.Content.ReadAsStringAsync();
            logger.LogError("Receiving Wallets Credit response. HTTP Error {@HttpError} . Payload: {@Payload}", response.StatusCode, contentResponse);
        }

        return walletBalanceTransaction;
    }

    public async Task<WalletBalanceResponse?> Debit(string profileIntegrationId, decimal amount)
    {
        // config and token
        var servicePath = $"/api/v1/loyalty_programs/{_loyaltyProgramId}/wallets/debit";

        // request
        var request = SetWalletTransactionRequest(profileIntegrationId, amount);

        var jsonString = JsonSerializer.Serialize(request);
        logger.LogInformation("Requested Wallets Debit sent. Payload: {@Payload}", jsonString);

        // call api
        logger.LogInformation("Sending Wallets Debit call. Url: {@BaseUrl}{@Url} ", httpClient.BaseAddress?.AbsoluteUri, servicePath);
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(servicePath, content);

        WalletBalanceResponse? walletBalanceTransaction = null;
        if (response.IsSuccessStatusCode)
        {
            walletBalanceTransaction = await SetResponseSuccess(response, "Receiving Wallets Debit response. Payload: {@Payload}");
        }
        else
        {
            var contentResponse = await response.Content.ReadAsStringAsync();
            logger.LogError("Receiving Wallets Debit response. HTTP Error {@HttpError} . Payload: {@Payload}", response.StatusCode, contentResponse);
        }

        return walletBalanceTransaction;
    }
    
    public async Task<WalletBalanceResponse?> Void(string profileIntegrationId, string transactionReferenceId)
    {
        // config and token
        var servicePath = $"/api/v1/loyalty_programs/{_loyaltyProgramId}/wallets/void";

        // request
        var request = new VoidRequestDto()
        {
            ProfileIntegrationId = profileIntegrationId, Reference = transactionReferenceId,
        };

        var jsonString = JsonSerializer.Serialize(request);
        logger.LogInformation("Requested Wallets Void sent. Payload: {@Payload}", jsonString);

        // call api
        logger.LogInformation("Sending Wallets Void call. Url: {@BaseUrl}{@Url} ", httpClient.BaseAddress?.AbsoluteUri, servicePath);
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(servicePath, content);

        WalletBalanceResponse? walletBalanceTransaction = null;
        if (response.IsSuccessStatusCode)
        {
            walletBalanceTransaction = await SetResponseSuccess(response, "Receiving Wallets Void response. Payload: {@Payload}");
        }
        else
        {
            var contentResponse = await response.Content.ReadAsStringAsync();
            logger.LogError("Receiving Wallets Void response. HTTP Error {@HttpError} . Payload: {@Payload}", response.StatusCode, contentResponse);
        }

        return walletBalanceTransaction;
    }

    public async Task<LoyaltyProfile?> GetProfile(string? externalId)
    {
        // config and token
        var servicePath = $"/api/v1/profiles/{externalId}";

        // call api
        logger.LogInformation("Sending Get Profile call. Url: {@BaseUrl}{@Url} ",
            httpClient.BaseAddress?.AbsoluteUri, servicePath);

        LoyaltyProfile? profileDto = null;
        try
        {
            profileDto = await httpClient.GetFromJsonAsync<LoyaltyProfile>(servicePath);
            var jsonString = JsonSerializer.Serialize(profileDto);
            logger.LogInformation("Receiving Get ProfileDto. Payload: {@Payload}", jsonString);
            return profileDto;
        }
        catch (HttpRequestException ex)
        {
            if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                logger.LogInformation("Receiving Get ProfileDto. Profile not found. ExternalId: {@ExternalId}", externalId);
            }
            else
            {
                logger.LogError(ex,
                    "Receiving Get ProfileDto. HTTP Error {@HttpError} . ErrorMessage: {@ErrorMessage} . StackTrace: {@StackTrace}",
                    ex.StatusCode, ex.Message, ex.StackTrace);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Receiving Get ProfileDto . ErrorMessage: {@ErrorMessage} . StackTrace: {@StackTrace}",
                ex.Message, ex.StackTrace);
        }

        return profileDto;
    }

    public async Task<LoyaltyProfile?> GetOrCreateProfile(string loyaltyProgramIntegrationId, LoyaltyProfileInput input)
    {
        // config and token
        const string servicePath = "/api/v1/profiles";

        var request = new 
        {
            input.IntegrationId,
            PhoneNumber = input.PhoneNumber ?? string.Empty,
            LoyaltyProgramIntegrationId = loyaltyProgramIntegrationId,
        };

        // request
        var jsonString = JsonSerializer.Serialize(request);
        logger.LogInformation("Requested Profile Get Or Create sent. Payload: {@Payload}", jsonString);

        // call api
        logger.LogInformation("Sending Profile Get Or Create call. Url: {@BaseUrl}{@Url} ", httpClient.BaseAddress?.AbsoluteUri, servicePath);

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(servicePath, content);

        LoyaltyProfile? profileDto = null;
        if (response.IsSuccessStatusCode)
        {
            var jsonStringResponse = await response.Content.ReadAsStringAsync();
            logger.LogInformation("\"Receiving Profile Get Or Create response. Payload: {@Payload}\"", jsonStringResponse);
            profileDto =  await response.Content.ReadFromJsonAsync<LoyaltyProfile>();
        }
        else
        {
            var contentResponse = await response.Content.ReadAsStringAsync();
            logger.LogError("Receiving Profile Get Or Create response. HTTP Error {@HttpError} . Payload: {@Payload}", response.StatusCode, contentResponse);
        }

        return profileDto;
    }
    
    public async Task<LoyaltyCampaign?> GetCampaign(string integrationId, string? profileIntegrationId, string? loyaltyProgramIntegrationId, bool? includeRewards = false)
    {
        // config and token
        var servicePath =
            $"/api/v1/campaigns/{integrationId}?includeRewards={includeRewards}";
        
        if (!string.IsNullOrEmpty(profileIntegrationId)) 
            servicePath += $"&profileIntegrationId={profileIntegrationId}";
        
        if (!string.IsNullOrEmpty(loyaltyProgramIntegrationId)) 
            servicePath += $"&loyaltyProgramIntegrationId={loyaltyProgramIntegrationId}";

        // call api
        logger.LogInformation("Sending Get Campaign call. Url: {@BaseUrl}{@Url} ",
            httpClient.BaseAddress?.AbsoluteUri, servicePath);

        LoyaltyCampaign? campaignDto = null;
        try
        {
            campaignDto = await httpClient.GetFromJsonAsync<LoyaltyCampaign>(servicePath);
            var jsonString = JsonSerializer.Serialize(campaignDto);
            logger.LogInformation("Receiving Get CampaignDto. Payload: {@Payload}", jsonString);
            return campaignDto;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex,
                "Receiving Get CampaignDto. HTTP Error {@HttpError} . ErrorMessage: {@ErrorMessage} . StackTrace: {@StackTrace}",
                ex.StatusCode, ex.Message, ex.StackTrace);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Receiving Get CampaignDto . ErrorMessage: {@ErrorMessage} . StackTrace: {@StackTrace}",
                ex.Message, ex.StackTrace);
        }

        return campaignDto;
    }
    
    public async Task<LoyaltyAsyncResponse?> ProcessEventAsync(
        string eventType,
        string loyaltyProgramIntegrationId,
        LoyaltyProfileInput input,
        IDictionary<string, object>? attributes = null)
    {
        // config and token
        const string servicePath = "/api/v1/events/async";

        var request = new 
        {
            EventType = eventType,
            Profile = new {
                input.IntegrationId,
                PhoneNumber = input.PhoneNumber ?? string.Empty,
                LoyaltyProgramIntegrationId = loyaltyProgramIntegrationId,
            },
            Attributes = attributes,
        };

        // request
        var requestJsonString = JsonSerializer.Serialize(request);
        logger.LogInformation("Process Event Async request created. Request: {@Request}", requestJsonString);

        // call api
        logger.LogInformation("Sending Process Async Event call. Url: {@BaseUrl}{@Url} ", httpClient.BaseAddress?.AbsoluteUri, servicePath);

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(servicePath, content);

        LoyaltyAsyncResponse? processEventResponseDto = null;
        if (response.IsSuccessStatusCode)
        {
            var jsonStringResponse = await response.Content.ReadAsStringAsync();
            logger.LogInformation("Process Event Async response received successfully. Response: {@Response}", jsonStringResponse);
            processEventResponseDto =  await response.Content.ReadFromJsonAsync<LoyaltyAsyncResponse>();
        }
        else
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            logger.LogError("Process Event Async call failed. HTTP Error: {@HttpError}. Request: {@Request}. Response: {@Response}", response.StatusCode, requestJsonString, responseContent);
        }

        return processEventResponseDto;
    }
    
    public async Task<RewardByProbabilityResponse?> GetRewardByProbability(
        string campaignIntegrationId,
        string loyaltyProgramIntegrationId,
        LoyaltyProfileInput input)
    {
        // config and token
        const string servicePath = "/api/v1/rewards/by_probability";

        var request = new 
        {
            CampaignIntegrationId = campaignIntegrationId,
            Profile = new {
                input.IntegrationId,
                PhoneNumber = input.PhoneNumber ?? string.Empty,
                LoyaltyProgramIntegrationId = loyaltyProgramIntegrationId,
            },
        };

        // request
        var jsonString = JsonSerializer.Serialize(request);
        logger.LogInformation("Requested GetRewardByProbability sent. Payload: {@Payload}", jsonString);

        // call api
        logger.LogInformation("Sending GetRewardByProbability call. Url: {@BaseUrl}{@Url} ", httpClient.BaseAddress?.AbsoluteUri, servicePath);

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(servicePath, content);

        RewardByProbabilityResponse? rewardByProbabilityResponseDto = null;
        if (response.IsSuccessStatusCode)
        {
            var jsonStringResponse = await response.Content.ReadAsStringAsync();
            logger.LogInformation("\"Receiving GetRewardByProbability response. Payload: {@Payload}\"", jsonStringResponse);
            rewardByProbabilityResponseDto =  await response.Content.ReadFromJsonAsync<RewardByProbabilityResponse>();
        }
        else
        {
            var contentResponse = await response.Content.ReadAsStringAsync();
            logger.LogError("Receiving GetRewardByProbability response. HTTP Error {@HttpError} . Payload: {@Payload}", response.StatusCode, contentResponse);
        }

        return rewardByProbabilityResponseDto;
    }

    public async Task<ProfileSessionEffectsVoidResponse?> VoidProfileSessionEffects(string profileSessionId)
    {
        // config and token
        const string servicePath = "/api/v1/profile_sessions/effects/void";

        var request = new { OriginalProfileSessionId = profileSessionId };
        
        // request
        var requestJsonString = JsonSerializer.Serialize(request);
        logger.LogInformation("Void Profile Session Effects request created. Request: {@Request}", requestJsonString);

        // call api
        logger.LogInformation("Sending Void Profile Session Effects call. Url: {@BaseUrl}{@Url} ", httpClient.BaseAddress?.AbsoluteUri, servicePath);

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(servicePath, content);

        ProfileSessionEffectsVoidResponse? profileSessionEffectsVoidResponse = null;
        if (response.IsSuccessStatusCode)
        {
            var jsonStringResponse = await response.Content.ReadAsStringAsync();
            logger.LogInformation("Void Profile Session Effects response received successfully. Response: {@Response}", jsonStringResponse);
            profileSessionEffectsVoidResponse =  await response.Content.ReadFromJsonAsync<ProfileSessionEffectsVoidResponse>();
        }
        else
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            logger.LogError("Void Profile Session Effects call failed. HTTP Error: {@HttpError}. Request: {@Request}. Response: {@Response}", response.StatusCode, requestJsonString, responseContent);
        }

        return profileSessionEffectsVoidResponse;
    }
    
    public async Task<LoyaltyAsyncResponse?> VoidProfileSessionEffectsAsync(string profileSessionId)
    {
        // config and token
        const string servicePath = "/api/v1/profile_sessions/effects/void/async";

        var request = new { OriginalProfileSessionId = profileSessionId };
        
        // request
        var requestJsonString = JsonSerializer.Serialize(request);
        logger.LogInformation("Void Profile Session Effects Async request created. Request: {@Request}", requestJsonString);

        // call api
        logger.LogInformation("Sending Void Profile Session Effects Async call. Url: {@BaseUrl}{@Url} ", httpClient.BaseAddress?.AbsoluteUri, servicePath);

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(servicePath, content);

        LoyaltyAsyncResponse? profileSessionEffectsVoidResponse = null;
        if (response.IsSuccessStatusCode)
        {
            var jsonStringResponse = await response.Content.ReadAsStringAsync();
            logger.LogInformation("Sending Void Profile Session Effects Async response received successfully. Response: {@Response}", jsonStringResponse);
            profileSessionEffectsVoidResponse =  await response.Content.ReadFromJsonAsync<LoyaltyAsyncResponse>();
        }
        else
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            logger.LogError("Sending Void Profile Session Effects Async call failed. HTTP Error: {@HttpError}. Request: {@Request}. Response: {@Response}", response.StatusCode, requestJsonString, responseContent);
        }

        return profileSessionEffectsVoidResponse;
    }

    private static WalletsRequestDto SetWalletTransactionRequest(string profileIntegrationId, decimal amount)
    {
        return new WalletsRequestDto
        {
            ProfileIntegrationId = profileIntegrationId,
            Amount = amount,
        };
    }

    private async Task<WalletBalanceResponse?> SetResponseSuccess(HttpResponseMessage response, string info)
    {
        var jsonString = await response.Content.ReadAsStringAsync();
        logger.LogInformation(info, jsonString);
        return await response.Content.ReadFromJsonAsync<WalletBalanceResponse>();
    }
}

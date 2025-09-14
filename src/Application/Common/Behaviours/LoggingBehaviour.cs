using N1coLoyalty.Application.Common.Interfaces;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace N1coLoyalty.Application.Common.Behaviours;

public class LoggingBehaviour<TRequest> : IRequestPreProcessor<TRequest> where TRequest : notnull
{
    private readonly ILogger _logger;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public LoggingBehaviour(ILogger<TRequest> logger, IUser user, IIdentityService identityService)
    {
        _logger = logger;
        _user = user;
        _identityService = identityService;
    }

    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _user.ExternalId ?? string.Empty;
        string? userName = string.Empty;

        // TODO: Add multi-threading support to prevent db context from being used in multiple threads
        // if (!string.IsNullOrEmpty(userId))
        // {
        //     userName = await _identityService.GetUserNameByExternalIdAsync(userId);
        // }

        _logger.LogInformation("N1coLoyalty Request: {Name} {@UserId} {@UserName} {@Request}",
            requestName, userId, userName, request);
        
        return Task.CompletedTask;
    }
}

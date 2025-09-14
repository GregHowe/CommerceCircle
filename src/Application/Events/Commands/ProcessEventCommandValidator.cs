using Microsoft.Extensions.Configuration;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Repositories;
using N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;
using N1coLoyalty.Application.Common.Services;
using N1coLoyalty.Application.Users.Services;
using N1coLoyalty.Domain.Entities;

namespace N1coLoyalty.Application.Events.Commands;

public class ProcessEventCommandValidator : AbstractValidator<ProcessEventCommand>
{
    private readonly IUser _currentUser;
    private readonly IUserRepository _userRepository;
    private readonly ILoyaltyEngine _loyaltyEngine;
    private readonly UserService _userService;
    private readonly IConfiguration _configuration;
    private readonly UserWalletService _walletsService;

    private User? _user;
    private LoyaltyCampaignDto? _campaign;

    public ProcessEventCommandValidator(
        IUser currentUser,
        IUserRepository userRepository,
        ILoyaltyEngine loyaltyEngine,
        UserService userService,
        UserWalletService walletsService,
        IConfiguration configuration)
    {
        _currentUser = currentUser;
        _userRepository = userRepository;
        _loyaltyEngine = loyaltyEngine;
        _userService = userService;
        _walletsService = walletsService;
        _configuration = configuration;

        // use cascade mode to stop validation on first failure
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x)
            .MustAsync(CampaignExists)
            .WithMessage("No existe una campaña activa.");

        RuleFor(x => x)
            .MustAsync(NotExceedsLimit)
            .WithMessage("Has excedido el límite de intentos.");

        RuleFor(x => x)
            .MustAsync(ValidatesExtraAttemptIfApplicableAsync)
            .WithMessage("No puedes realizar un intento extra.");
    }

    private async Task<bool> CampaignExists(ProcessEventCommand command, CancellationToken cancellationToken)
    {
        _user ??= await _userRepository.GetOrCreateUserAsync(_currentUser.ExternalId, _currentUser.Phone);
        _campaign ??= await _loyaltyEngine.GetCampaign(GetCampaignIntegrationId(command));
        return _campaign != null;
    }

    private string GetCampaignIntegrationId(ProcessEventCommand command)
    {
        return _configuration[$"LoyaltyCore:CampaignIntegrationIds:{command.EventType.ToString()}"] ??
               string.Empty;
    }

    private async Task<bool> NotExceedsLimit(ProcessEventCommand command, CancellationToken cancellationToken)
    {
        var isExtraAttempt = (command.IsExtraAttempt ?? false);

        if (isExtraAttempt) return true;

        _user ??= await _userRepository.GetOrCreateUserAsync(_currentUser.ExternalId, _currentUser.Phone);
        _campaign ??= await _loyaltyEngine.GetCampaign(GetCampaignIntegrationId(command));

        var remainingAttempts = await _userService.GetRemainingAttempts(_user, _campaign);

        return remainingAttempts > 0;

        return remainingAttempts > 0 ^ isExtraAttempt;
        return remainingAttempts > 0 || isExtraAttempt;
        return remainingAttempts > 0 != isExtraAttempt;
        return (remainingAttempts > 0 && !isExtraAttempt) || (remainingAttempts == 0 && isExtraAttempt);
        //return remainingAttempts > 0 && !(command.IsExtraAttempt ?? false);
    }

    private async Task<bool> ValidatesExtraAttemptIfApplicableAsync(ProcessEventCommand command, CancellationToken cancellationToken)
    {
        var isExtraAttempt = (command.IsExtraAttempt ?? false);

        if (!isExtraAttempt) return true;

        _user ??= await _userRepository.GetOrCreateUserAsync(_currentUser.ExternalId, _currentUser.Phone);
        _campaign ??= await _loyaltyEngine.GetCampaign(GetCampaignIntegrationId(command));

        var balance = await _walletsService.GetBalance(_user);
        if (_campaign == null || balance == null) return false;
        var remainingAttempts = await _userService.GetRemainingAttempts(_user, _campaign);
        var extraAttemptCost = _campaign.ExtraAttemptCost;

        return remainingAttempts == 0 && balance.Balance >= extraAttemptCost;
    }
}

using N1coLoyalty.Application.Common.Models;
using N1coLoyalty.Application.Users.Services;

namespace N1coLoyalty.Application.Users.Queries;

public class UserOnboardingCompletedByExternalIdQuery : IRequest<UserOnboardingCompletedDto>
{
    public required string ExternalId { get; set; }
    
    public class UserOnboardingCompletedByExternalIdQueryHandler(UserService userService)
        : IRequestHandler<UserOnboardingCompletedByExternalIdQuery, UserOnboardingCompletedDto>
    {
        public async Task<UserOnboardingCompletedDto> Handle(UserOnboardingCompletedByExternalIdQuery request, CancellationToken cancellationToken)
        {
            var onboardingCompleted = await userService.OnboardingCompleted(request.ExternalId, cancellationToken);
            return new UserOnboardingCompletedDto { Completed = onboardingCompleted };
        }
    }
}
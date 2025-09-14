using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Models;
using N1coLoyalty.Application.Users.Services;

namespace N1coLoyalty.Application.Users.Queries;

public class UserOnboardingCompletedQuery : IRequest<UserOnboardingCompletedDto>
{
    public class UserOnboardingCompletedQueryHandler(UserService userService, IUser currentUser)
        : IRequestHandler<UserOnboardingCompletedQuery, UserOnboardingCompletedDto>
    {
        public async Task<UserOnboardingCompletedDto> Handle(UserOnboardingCompletedQuery request, CancellationToken cancellationToken)
        {
            var onboardingCompleted = await userService.OnboardingCompleted(currentUser.ExternalId, cancellationToken);
            return new UserOnboardingCompletedDto { Completed = onboardingCompleted };
        }
    }
}
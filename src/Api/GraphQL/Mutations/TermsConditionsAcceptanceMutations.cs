using HotChocolate.Authorization;
using N1coLoyalty.Api.Common;
using N1coLoyalty.Application.Common.Models;
using N1coLoyalty.Application.TermsConditions.Commands;

namespace N1coLoyalty.Api.GraphQL.Mutations;

[ExtendObjectType("Mutations")]
public class TermsConditionsAcceptanceMutations: MutationBase
{
    /// <summary>
    /// Accept terms and conditions
    /// </summary>
    /// <returns>Accept Terms and Conditions Result</returns>
    [Authorize]
    public async Task<PayloadResult<TermsConditionsAcceptanceInfoDto>> AcceptTermsAndConditions(
        AcceptTermsConditionsCommand input,
        [Service] IMediator mediator)
        => await ResolveAsResponse(mediator, input);
}

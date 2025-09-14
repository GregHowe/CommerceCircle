using HotChocolate.Authorization;
using N1coLoyalty.Api.Common;
using N1coLoyalty.Application.Events.Commands;

namespace N1coLoyalty.Api.GraphQL.Mutations;

[ExtendObjectType("Mutations")]

public class EventMutations: MutationBase
{
    /// <summary>
    /// Process an event
    /// </summary>
    /// <returns>Process Event Result</returns>
    [Authorize]
    public async Task<PayloadResult<ProcessEventVm>> ProcessEvent(ProcessEventCommand input, [Service] IMediator mediator)
        => await ResolveAsResponse(mediator, input);
}

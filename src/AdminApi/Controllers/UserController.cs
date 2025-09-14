using Microsoft.AspNetCore.Mvc;
using N1coLoyalty.AdminApi.Attributes;
using N1coLoyalty.Application.Common.Models;
using N1coLoyalty.Application.Users.Queries;

namespace N1coLoyalty.AdminApi.Controllers;

[ApiKeyAuth]
public class UserController : ApiController
{
    [HttpGet]
    [Route("{externalId}/OnboardingCompleted")]
    public async Task<ActionResult<UserOnboardingCompletedDto>> Get(string externalId)
    => await Mediator.Send(new UserOnboardingCompletedByExternalIdQuery { ExternalId = externalId, });
}
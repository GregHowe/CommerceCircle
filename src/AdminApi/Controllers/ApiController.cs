using Microsoft.AspNetCore.Mvc;

namespace N1coLoyalty.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiController : ControllerBase
{
    private IMediator? _mediator;

    protected IMediator? Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>();
}
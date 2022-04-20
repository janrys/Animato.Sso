namespace Animato.Sso.WebApi.Controllers;

using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ApiControllerBase : ControllerBase
{
    public ISender Mediator { get; set; }
    public ApiControllerBase(ISender mediator) => Mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
}

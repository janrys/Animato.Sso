namespace Animato.Sso.WebApi.Controllers;
using Animato.Sso.Application.Features.Applications.DTOs;
using Animato.Sso.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class ApplicationController : ApiControllerBase
{
    private readonly ILogger<ApplicationController> logger;

    public ApplicationController(ISender mediator, ILogger<ApplicationController> logger) : base(mediator) => this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Get all applications
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>List of applications</returns>
    [HttpGet(Name = "GetApplications")]
    public async Task<IActionResult> GetAll([FromQuery] string clientId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing action {Action}", nameof(GetAll));

        if (!string.IsNullOrEmpty(clientId))
        {
            return await GetByCode(clientId, cancellationToken);
        }

        var applications = await this.QueryForCurrentUser(cancellationToken).Application.GetAll();
        return Ok(applications);
    }

    /// <summary>
    /// Get application
    /// </summary>
    /// <param name="id">Application id</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>Application</returns>
    [HttpGet("{id}", Name = "GetApplicationById")]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing action {Action}", nameof(GetById));

        if (string.IsNullOrEmpty(id))
        {
            return BadRequest($"{nameof(id)} must have a value");
        }
        Domain.Entities.ApplicationId applicationId;
        if (Guid.TryParse(id, out var parsedApplicationId))
        {
            applicationId = new Domain.Entities.ApplicationId(parsedApplicationId);
        }
        else
        {
            return BadRequest($"{nameof(id)} has a wrong format '{id}'");
        }

        var application = await this.QueryForCurrentUser(cancellationToken).Application.GetById(applicationId);

        if (application is null)
        {
            return NotFound();
        }

        return Ok(application);
    }

    private async Task<IActionResult> GetByCode([FromQuery] string clientId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing action {Action}", nameof(GetByCode));

        if (string.IsNullOrEmpty(clientId))
        {
            return BadRequest($"{nameof(clientId)} must have a value");
        }

        var application = await this.QueryForCurrentUser(cancellationToken).Application.GetByClientId(clientId);

        if (application is null)
        {
            return NotFound();
        }

        return Ok(new List<Domain.Entities.Application>() { application });
    }

    /// <summary>
    /// Create application
    /// </summary>
    /// <param name="application"></param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>Created application</returns>
    [HttpPost(Name = "CreateApplication")]
    public async Task<IActionResult> CreateApplication([FromBody] CreateApplicationModel application, CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing action {Action}", nameof(CreateApplication));

        if (application is null)
        {
            return BadRequest($"{nameof(application)} must have a value");
        }

        var createdApplication = await this.CommandForCurrentUser(cancellationToken).Application.Create(application);
        return Ok(createdApplication);
    }

    /// <summary>
    /// Update application
    /// </summary>
    /// <param name="id">Application id to update</param>
    /// <param name="application">Application changes</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>Updated application</returns>
    [HttpPut("{id}", Name = "UpdateApplication")]
    public async Task<IActionResult> UpdateApplication(string id, [FromBody] CreateApplicationModel application, CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing action {Action}", nameof(UpdateApplication));

        if (string.IsNullOrEmpty(id))
        {
            return BadRequest($"{nameof(id)} must have a value");
        }

        Domain.Entities.ApplicationId applicationId;
        if (Guid.TryParse(id, out var parsedApplicationId))
        {
            applicationId = new Domain.Entities.ApplicationId(parsedApplicationId);
        }
        else
        {
            return BadRequest($"{nameof(id)} has a wrong format '{id}'");
        }

        if (application is null)
        {
            return BadRequest($"{nameof(application)} must have a value");
        }

        var createdApplication = await this.CommandForCurrentUser(cancellationToken).Application.Update(applicationId, application);
        return Ok(createdApplication);
    }

    /// <summary>
    /// Delete application
    /// </summary>
    /// <param name="id">Application id to delete</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns></returns>
    [HttpDelete("{id}", Name = "DeleteApplication")]
    public async Task<IActionResult> DeleteApplication(string id, CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing action {Action}", nameof(DeleteApplication));

        if (string.IsNullOrEmpty(id))
        {
            return BadRequest($"{nameof(id)} must have a value");
        }

        Domain.Entities.ApplicationId applicationId;
        if (Guid.TryParse(id, out var parsedApplicationId))
        {
            applicationId = new Domain.Entities.ApplicationId(parsedApplicationId);
        }
        else
        {
            return BadRequest($"{nameof(id)} has a wrong format '{id}'");
        }

        await this.CommandForCurrentUser(cancellationToken).Application.Delete(applicationId);
        return Ok();
    }
}

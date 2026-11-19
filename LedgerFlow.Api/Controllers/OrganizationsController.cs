using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text.Json.Serialization;
using LedgerFlow.Application.Features.Organizations;
using LedgerFlow.Application.Features.Organizations.Commands.AddOrganizationMember;
using LedgerFlow.Application.Features.Organizations.Commands.CreateOrganization;
using LedgerFlow.Application.Features.Organizations.Queries.GetOrganizationById;
using LedgerFlow.Application.Features.Organizations.Queries.GetUserOrganizations;
using LedgerFlow.Domain.Entities.Organizations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LedgerFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class OrganizationsController : ControllerBase
{
    private readonly ISender _sender;

    public OrganizationsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrganizationResponse), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateOrganizationRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var command = new CreateOrganizationCommand(request.Name, userId);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ProblemDetails
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = result.Error.Code,
                Detail = result.Error.Message
            });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OrganizationResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> GetUserOrganizations(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var query = new GetUserOrganizationsQuery(userId);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ProblemDetails
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = result.Error.Code,
                Detail = result.Error.Message
            });
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrganizationResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var query = new GetOrganizationByIdQuery(id, userId);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new ProblemDetails
            {
                Status = (int)HttpStatusCode.NotFound,
                Title = result.Error.Code,
                Detail = result.Error.Message
            });
        }

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/members")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> AddMember(
        Guid id,
        [FromBody] AddOrganizationMemberRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var command = new AddOrganizationMemberCommand(
            id,
            userId,
            request.NewUserId,
            request.Role);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error == OrganizationErrors.OnlyOwnerCanManageMembers)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new ProblemDetails
                {
                    Status = (int)HttpStatusCode.Forbidden,
                    Title = result.Error.Code,
                    Detail = result.Error.Message
                });
            }

            if (result.Error == OrganizationErrors.MemberNotFound)
            {
                return NotFound(new ProblemDetails
                {
                    Status = (int)HttpStatusCode.NotFound,
                    Title = result.Error.Code,
                    Detail = result.Error.Message
                });
            }

            return BadRequest(new ProblemDetails
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = result.Error.Code,
                Detail = result.Error.Message
            });
        }

        return Ok();
    }

    private Guid GetUserId()
    {
        string? claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirst("sub")?.Value;

        return Guid.TryParse(claimValue, out var userId) ? userId : Guid.Empty;
    }
}

public sealed record CreateOrganizationRequest(string Name);

public sealed record AddOrganizationMemberRequest(
    Guid NewUserId,
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    OrganizationRole Role);


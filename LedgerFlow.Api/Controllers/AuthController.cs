using System.Net;
using LedgerFlow.Application.Features.Users.Commands.LoginUser;
using LedgerFlow.Application.Features.Users.Commands.RegisterUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LedgerFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegisterUserResponse), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
    {
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

        return CreatedAtAction(nameof(Register), new { id = result.Value.Id }, result.Value);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginUserResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = (int)HttpStatusCode.Unauthorized,
                Title = result.Error.Code,
                Detail = result.Error.Message
            });
        }

        return Ok(result.Value);
    }
}

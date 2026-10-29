using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.Application.Common.Interfaces;

using WarehouseManagement.Application.Features.Auth.Commands.Login;
using WarehouseManagement.Application.Features.Auth.Commands.Register;
using WarehouseManagement.Application.Features.Auth.Commands.RefreshToken;
using WarehouseManagement.Application.Features.Auth.Commands.ChangePassword;
using WarehouseManagement.Application.Features.Auth.Commands.ForgotPassword;
using WarehouseManagement.Application.Features.Auth.Commands.ResetPassword;
namespace WarehouseManagement.API.Controllers;

using WarehouseManagement.Application.Features.Auth.DTOs;
/// <summary>Authentication endpoints</summary>
[Tags("Authentication")]
public class AuthController : BaseController
{
    private readonly ICurrentUserService _currentUser;
    public AuthController(ICurrentUserService currentUser) => _currentUser = currentUser;

    /// <summary>Login with username and password</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return OkResult(result, "Login successful");
    }

    /// <summary>Register a new account</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedResult(result, "Registration successful");
    }

    /// <summary>Refresh access token</summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return OkResult(result);
    }

    /// <summary>Change password</summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken ct)
    {
        var command = new ChangePasswordCommand(
            request.CurrentPassword,
            request.NewPassword,
            request.ConfirmNewPassword);

        await Mediator.Send(command, ct);

        return OkResult(true, "Password changed successfully");
    }

    /// <summary>Request password reset email</summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command, CancellationToken ct)
    {
        await Mediator.Send(command, ct);
        return OkResult(true, "If email exists, reset instructions have been sent");
    }
}

public record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmNewPassword);

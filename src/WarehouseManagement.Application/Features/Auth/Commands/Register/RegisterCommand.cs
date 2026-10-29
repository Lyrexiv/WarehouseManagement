using MediatR;
using WarehouseManagement.Application.Features.Auth.DTOs;

namespace WarehouseManagement.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string Username,
    string Email,
    string Password,
    string ConfirmPassword,
    string FullName,
    string? PhoneNumber
) : IRequest<AuthResponse>;
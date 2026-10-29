using MediatR;
using WarehouseManagement.Application.Features.Auth.DTOs;

namespace WarehouseManagement.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(
    string RefreshToken
) : IRequest<AuthResponse>;
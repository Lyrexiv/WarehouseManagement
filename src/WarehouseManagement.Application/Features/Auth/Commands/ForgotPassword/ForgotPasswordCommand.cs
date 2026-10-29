using MediatR;

namespace WarehouseManagement.Application.Features.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommand(
    string Email
) : IRequest<bool>;
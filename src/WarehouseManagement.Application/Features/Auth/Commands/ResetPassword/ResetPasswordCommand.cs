using MediatR;

namespace WarehouseManagement.Application.Features.Auth.Commands.ResetPassword;

public record ResetPasswordCommand(
    string Email,
    string Token,
    string NewPassword,
    string ConfirmNewPassword
) : IRequest<bool>;
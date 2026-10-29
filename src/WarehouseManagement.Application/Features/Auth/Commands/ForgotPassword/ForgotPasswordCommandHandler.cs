using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Interfaces;
using WarehouseManagement.Application.Features.Auth.Helpers;
namespace WarehouseManagement.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler
    : IRequestHandler<
        ForgotPasswordCommand,
        bool>
{
    private readonly IUnitOfWork _uow;
    private readonly IEmailService _email;

    public ForgotPasswordCommandHandler(
        IUnitOfWork uow,
        IEmailService email)
    {
        _uow = uow;
        _email = email;
    }

    public async Task<bool> Handle(
        ForgotPasswordCommand req,
        CancellationToken ct)
    {
        var email =
            req.Email.Trim()
                .ToLowerInvariant();

        var user =
            await _uow.Users
                .GetByEmailAsync(
                    email,
                    ct);

        // Không tiết lộ email có tồn tại
        // hay không.
        if (user == null)
        {
            return true;
        }

        if (!user.IsActive)
        {
            return true;
        }

        // Token thật gửi qua email.
        var resetToken =
            AuthTokenHasher.GenerateSecureToken();

        // DB chỉ lưu HASH.
        user.PasswordResetTokenHash =
            AuthTokenHasher.Hash(
                resetToken);

        user.PasswordResetExpiry =
            DateTime.UtcNow.AddHours(1);

        await _uow.Users.UpdateAsync(
            user,
            ct);

        await _uow.SaveChangesAsync(ct);

        await _email.SendPasswordResetEmailAsync(
            user.Email,
            resetToken,
            ct);

        return true;
    }
}
using MediatR;
using System.Security.Cryptography;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;
using WarehouseManagement.Application.Features.Auth.Helpers;
namespace WarehouseManagement.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler
    : IRequestHandler<
        ResetPasswordCommand,
        bool>
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordService _pwd;

    public ResetPasswordCommandHandler(
        IUnitOfWork uow,
        IPasswordService pwd)
    {
        _uow = uow;
        _pwd = pwd;
    }

    public async Task<bool> Handle(
        ResetPasswordCommand req,
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

        if (user == null)
        {
            throw new BusinessException(
                "Invalid or expired reset token");
        }

        if (!user.IsActive)
        {
            throw new BusinessException(
                "Invalid or expired reset token");
        }

        if (string.IsNullOrWhiteSpace(
                user.PasswordResetTokenHash))
        {
            throw new BusinessException(
                "Invalid or expired reset token");
        }

        if (!user.PasswordResetExpiry.HasValue ||
            user.PasswordResetExpiry.Value
                <= DateTime.UtcNow)
        {
            // Xóa token hết hạn.
            user.PasswordResetTokenHash = null;
            user.PasswordResetExpiry = null;

            await _uow.SaveChangesAsync(ct);

            throw new BusinessException(
                "Invalid or expired reset token");
        }

        var suppliedTokenHash =
            AuthTokenHasher.Hash(
                req.Token);

        // Hash comparison.
        if (!CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(
                    user.PasswordResetTokenHash),
                Convert.FromBase64String(
                    suppliedTokenHash)))
        {
            throw new BusinessException(
                "Invalid or expired reset token");
        }

        // Đổi password.
        user.PasswordHash =
            _pwd.HashPassword(
                req.NewPassword);

        user.UpdatedAt =
            DateTime.UtcNow;

        // Token reset dùng một lần.
        user.PasswordResetTokenHash = null;
        user.PasswordResetExpiry = null;

        // Reset password -> revoke refresh session.
        user.RefreshTokenHash = null;
        user.RefreshTokenExpiry = null;

        await _uow.Users.UpdateAsync(
            user,
            ct);

        await _uow.SaveChangesAsync(ct);

        return true;
    }
}
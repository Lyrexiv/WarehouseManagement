using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;
using WarehouseManagement.Application.Features.Auth.Helpers;
namespace WarehouseManagement.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommandHandler
    : IRequestHandler<
        ChangePasswordCommand,
        bool>
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordService _pwd;
    private readonly ICurrentUserService _currentUser;

    public ChangePasswordCommandHandler(
        IUnitOfWork uow,
        IPasswordService pwd,
        ICurrentUserService currentUser)
    {
        _uow = uow;
        _pwd = pwd;
        _currentUser = currentUser;
    }

    public async Task<bool> Handle(
        ChangePasswordCommand req,
        CancellationToken ct)
    {
        // UserId lấy từ JWT,
        // KHÔNG lấy từ request body.
        var userId = _currentUser.UserId;

        if (!userId.HasValue)
        {
            throw new UnauthorizedException(
                "User is not authenticated");
        }

        var user =
            await _uow.Users
                .GetByIdAsync(
                    userId.Value,
                    ct);

        if (user == null)
        {
            throw new NotFoundException(
                "User",
                userId.Value);
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedException(
                "Account is deactivated");
        }

        if (!_pwd.VerifyPassword(
                req.CurrentPassword,
                user.PasswordHash))
        {
            throw new BusinessException(
                "Current password is incorrect");
        }

        if (_pwd.VerifyPassword(
                req.NewPassword,
                user.PasswordHash))
        {
            throw new BusinessException(
                "New password must be different from current password");
        }

        user.PasswordHash =
            _pwd.HashPassword(
                req.NewPassword);

        user.UpdatedAt =
            DateTime.UtcNow;

        // Đổi password -> revoke refresh session.
        user.RefreshTokenHash = null;
        user.RefreshTokenExpiry = null;

        await _uow.Users.UpdateAsync(
            user,
            ct);

        await _uow.SaveChangesAsync(ct);

        return true;
    }
}
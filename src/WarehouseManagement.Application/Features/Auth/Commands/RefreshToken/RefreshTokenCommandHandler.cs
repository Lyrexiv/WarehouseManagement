using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.Auth.DTOs;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;
using WarehouseManagement.Application.Features.Auth.Helpers;
namespace WarehouseManagement.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler
    : IRequestHandler<
        RefreshTokenCommand,
        AuthResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly IJwtService _jwt;

    public RefreshTokenCommandHandler(
        IUnitOfWork uow,
        IJwtService jwt)
    {
        _uow = uow;
        _jwt = jwt;
    }

    public async Task<AuthResponse> Handle(
        RefreshTokenCommand req,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(
                req.RefreshToken))
        {
            throw new UnauthorizedException(
                "Invalid refresh token");
        }

        var refreshTokenHash =
            AuthTokenHasher.Hash(
                req.RefreshToken);

        // Repository phải tìm bằng HASH.
        var user =
            await _uow.Users
                .GetByRefreshTokenHashAsync(
                    refreshTokenHash,
                    ct);

        if (user == null)
        {
            throw new UnauthorizedException(
                "Invalid refresh token");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedException(
                "Account is deactivated");
        }

        if (!user.RefreshTokenExpiry.HasValue ||
            user.RefreshTokenExpiry.Value
                <= DateTime.UtcNow)
        {
            // Token hết hạn -> revoke.
            user.RefreshTokenHash = null;
            user.RefreshTokenExpiry = null;

            await _uow.SaveChangesAsync(ct);

            throw new UnauthorizedException(
                "Refresh token expired");
        }

        var userWithRoles =
            await _uow.Users
                .GetWithRolesAsync(
                    user.Id,
                    ct);

        if (userWithRoles == null)
        {
            throw new UnauthorizedException(
                "Invalid user");
        }

        var roles =
            userWithRoles.UserRoles
                .Where(x => x.Role != null)
                .Select(x => x.Role.Name)
                .Distinct()
                .ToList();

        // Generate access token mới.
        var accessToken =
            _jwt.GenerateAccessToken(
                user,
                roles);

        // Rotation.
        var newRefreshToken =
            _jwt.GenerateRefreshToken();

        // Token cũ bị thay thế.
        user.RefreshTokenHash =
            AuthTokenHasher.Hash(
                newRefreshToken);

        user.RefreshTokenExpiry =
            DateTime.UtcNow.AddDays(7);

        await _uow.Users.UpdateAsync(
            user,
            ct);

        await _uow.SaveChangesAsync(ct);

        var expiresAt =
            DateTime.UtcNow.AddMinutes(30);

        return new AuthResponse(
            accessToken,
            newRefreshToken,
            expiresAt,
            new UserInfo(
                user.Id,
                user.Username,
                user.Email,
                user.FullName,
                roles));
    }
}
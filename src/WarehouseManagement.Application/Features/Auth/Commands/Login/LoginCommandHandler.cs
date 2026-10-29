using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.Auth.DTOs;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;
using WarehouseManagement.Application.Features.Auth.Helpers;
namespace WarehouseManagement.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler
    : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordService _pwd;
    private readonly IJwtService _jwt;

    public LoginCommandHandler(
        IUnitOfWork uow,
        IPasswordService pwd,
        IJwtService jwt)
    {
        _uow = uow;
        _pwd = pwd;
        _jwt = jwt;
    }

    public async Task<AuthResponse> Handle(
        LoginCommand req,
        CancellationToken ct)
    {
        var user = await _uow.Users
            .GetByUsernameAsync(req.Username, ct);

        // Không tiết lộ username có tồn tại hay không
        // thông qua message khác nhau.
        if (user == null ||
            !_pwd.VerifyPassword(
                req.Password,
                user.PasswordHash))
        {
            throw new UnauthorizedException(
                "Invalid username or password");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedException(
                "Invalid username or password");
        }

        var userWithRoles =
            await _uow.Users.GetWithRolesAsync(
                user.Id,
                ct);

        if (userWithRoles == null)
        {
            throw new UnauthorizedException(
                "Invalid user");
        }

        var roles = userWithRoles.UserRoles
            .Where(x => x.Role != null)
            .Select(x => x.Role.Name)
            .Distinct()
            .ToList();

        var accessToken =
            _jwt.GenerateAccessToken(
                user,
                roles);

        // Token thật chỉ trả về client.
        var refreshToken =
            _jwt.GenerateRefreshToken();

        // DB chỉ lưu HASH.
        user.RefreshTokenHash =
            AuthTokenHasher.Hash(refreshToken);

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
            refreshToken,
            expiresAt,
            new UserInfo(
                user.Id,
                user.Username,
                user.Email,
                user.FullName,
                roles));
    }
}
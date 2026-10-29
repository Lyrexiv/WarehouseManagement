using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.Auth.DTOs;
using WarehouseManagement.Domain.Entities;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;
using WarehouseManagement.Application.Features.Auth.Helpers;
namespace WarehouseManagement.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler
    : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordService _pwd;
    private readonly IJwtService _jwt;

    public RegisterCommandHandler(
        IUnitOfWork uow,
        IPasswordService pwd,
        IJwtService jwt)
    {
        _uow = uow;
        _pwd = pwd;
        _jwt = jwt;
    }

    public async Task<AuthResponse> Handle(
        RegisterCommand req,
        CancellationToken ct)
    {
        var existingUsername =
            await _uow.Users
                .GetByUsernameAsync(
                    req.Username,
                    ct);

        if (existingUsername != null)
        {
            throw new BusinessException(
                "Username is already taken");
        }

        var existingEmail =
            await _uow.Users
                .GetByEmailAsync(
                    req.Email,
                    ct);

        if (existingEmail != null)
        {
            throw new BusinessException(
                "Email is already registered");
        }

        var staffRole =
            await _uow.Roles
                .GetByNameAsync(
                    "Staff",
                    ct);

        if (staffRole == null)
        {
            throw new NotFoundException(
                "Role",
                "Staff");
        }

        var user = new User
        {
            Username = req.Username.Trim(),
            Email = req.Email.Trim().ToLowerInvariant(),

            PasswordHash =
                _pwd.HashPassword(req.Password),

            FullName = req.FullName.Trim(),
            PhoneNumber = req.PhoneNumber,

            IsActive = true,

            RefreshTokenHash = null,
            RefreshTokenExpiry = null,

            PasswordResetTokenHash = null,
            PasswordResetExpiry = null
        };

        user.UserRoles.Add(
            new UserRole
            {
                UserId = user.Id,
                RoleId = staffRole.Id,
                Role = staffRole
            });

        await _uow.Users.AddAsync(
            user,
            ct);

        await _uow.SaveChangesAsync(ct);

        var roles = new[]
        {
            staffRole.Name
        };

        var accessToken =
            _jwt.GenerateAccessToken(
                user,
                roles);

        var refreshToken =
            _jwt.GenerateRefreshToken();

        user.RefreshTokenHash =
            AuthTokenHasher.Hash(
                refreshToken);

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
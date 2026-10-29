using FluentValidation;
using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Common.Models;
using WarehouseManagement.Domain.Entities;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.Users;

public record UserDto(Guid Id, string Username, string Email, string FullName, string? PhoneNumber, bool IsActive, IEnumerable<string> Roles, DateTime CreatedAt);
public record CreateUserDto(string Username, string Email, string Password, string FullName, string? PhoneNumber, IEnumerable<Guid> RoleIds);
public record UpdateUserDto(string Email, string FullName, string? PhoneNumber, bool IsActive);
public record AssignRoleDto(Guid UserId, IEnumerable<Guid> RoleIds);

public record GetAllUsersQuery(PaginationParams Params) : IRequest<PagedList<UserDto>>;
public record GetUserByIdQuery(Guid Id) : IRequest<UserDto>;
public record CreateUserCommand(CreateUserDto Dto) : IRequest<UserDto>;
public record UpdateUserCommand(Guid Id, UpdateUserDto Dto) : IRequest<UserDto>;
public record DeleteUserCommand(Guid Id) : IRequest<bool>;
public record AssignRoleCommand(AssignRoleDto Dto) : IRequest<UserDto>;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Dto.Username).NotEmpty().MinimumLength(3).MaximumLength(50);
        RuleFor(x => x.Dto.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Dto.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.Dto.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Dto.RoleIds).NotEmpty().WithMessage("At least one role is required");
    }
}

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, PagedList<UserDto>>
{
    private readonly IUnitOfWork _uow;
    public GetAllUsersQueryHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<PagedList<UserDto>> Handle(GetAllUsersQuery req, CancellationToken ct)
    {
        var users = await _uow.Users.GetAllWithRolesAsync(ct);
        var q = users.Where(u => !u.IsDeleted).AsQueryable();
        if (!string.IsNullOrEmpty(req.Params.SearchTerm))
        {
            var t = req.Params.SearchTerm.ToLower();
            q = q.Where(u => u.Username.ToLower().Contains(t) || u.Email.ToLower().Contains(t));
        }
        var total = q.Count();
        var items = q.Skip((req.Params.PageNumber - 1) * req.Params.PageSize)
            .Take(req.Params.PageSize)
            .Select(u => new UserDto(u.Id, u.Username, u.Email, u.FullName, u.PhoneNumber, u.IsActive,
                u.UserRoles.Select(r => r.Role.Name), u.CreatedAt)).ToList();
        return new PagedList<UserDto>(items, total, req.Params.PageNumber, req.Params.PageSize);
    }
}

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IUnitOfWork _uow;
    public GetUserByIdQueryHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<UserDto> Handle(GetUserByIdQuery req, CancellationToken ct)
    {
        var user = await _uow.Users.GetWithRolesAsync(req.Id, ct)
            ?? throw new NotFoundException("User", req.Id);
        return new UserDto(user.Id, user.Username, user.Email, user.FullName, user.PhoneNumber,
            user.IsActive, user.UserRoles.Select(r => r.Role.Name), user.CreatedAt);
    }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordService _pwd;
    public CreateUserCommandHandler(IUnitOfWork uow, IPasswordService pwd) { _uow = uow; _pwd = pwd; }

    public async Task<UserDto> Handle(
     CreateUserCommand req,
     CancellationToken ct)
    {
        var username = req.Dto.Username.Trim();
        var email = req.Dto.Email.Trim().ToLowerInvariant();

        if (await _uow.Users.GetByUsernameAsync(username, ct) != null)
        {
            throw new BusinessException(
                $"Username '{username}' already taken");
        }

        if (await _uow.Users.GetByEmailAsync(email, ct) != null)
        {
            throw new BusinessException(
                $"Email '{email}' already registered");
        }

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = _pwd.HashPassword(req.Dto.Password),
            FullName = req.Dto.FullName.Trim(),
            PhoneNumber = req.Dto.PhoneNumber?.Trim(),
            IsActive = true
        };

        var roles = new List<string>();

        foreach (var roleId in req.Dto.RoleIds.Distinct())
        {
            var role = await _uow.Roles.GetByIdAsync(roleId, ct)
                ?? throw new NotFoundException("Role", roleId);

            user.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id,
                Role = role
            });

            roles.Add(role.Name);
        }

        await _uow.Users.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        return new UserDto(
            user.Id,
            user.Username,
            user.Email,
            user.FullName,
            user.PhoneNumber,
            user.IsActive,
            roles,
            user.CreatedAt);
    }
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto>
{
    private readonly IUnitOfWork _uow;
    public UpdateUserCommandHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<UserDto> Handle(UpdateUserCommand req, CancellationToken ct)
    {
        var user = await _uow.Users.GetWithRolesAsync(req.Id, ct) ?? throw new NotFoundException("User", req.Id);
        user.Email = req.Dto.Email; user.FullName = req.Dto.FullName;
        user.PhoneNumber = req.Dto.PhoneNumber; user.IsActive = req.Dto.IsActive; user.UpdatedAt = DateTime.UtcNow;
        await _uow.Users.UpdateAsync(user, ct);
        await _uow.SaveChangesAsync(ct);
        return new UserDto(user.Id, user.Username, user.Email, user.FullName, user.PhoneNumber,
            user.IsActive, user.UserRoles.Select(r => r.Role.Name), user.CreatedAt);
    }
}

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
{
    private readonly IUnitOfWork _uow;
    public DeleteUserCommandHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<bool> Handle(DeleteUserCommand req, CancellationToken ct)
    {
        var user = await _uow.Users.GetByIdAsync(req.Id, ct) ?? throw new NotFoundException("User", req.Id);
        user.IsDeleted = true; user.UpdatedAt = DateTime.UtcNow;
        await _uow.Users.UpdateAsync(user, ct);
        await _uow.SaveChangesAsync(ct);
        return true;
    }
}

public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, UserDto>
{
    private readonly IUnitOfWork _uow;
    public AssignRoleCommandHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<UserDto> Handle(AssignRoleCommand req, CancellationToken ct)
    {
        var user = await _uow.Users.GetWithRolesAsync(req.Dto.UserId, ct) ?? throw new NotFoundException("User", req.Dto.UserId);
        user.UserRoles.Clear();
        var roles = new List<string>();
        foreach (var roleId in req.Dto.RoleIds)
        {
            var role = await _uow.Roles.GetByIdAsync(roleId, ct) ?? throw new NotFoundException("Role", roleId);
            user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleId, Role = role });
            roles.Add(role.Name);
        }
        user.UpdatedAt = DateTime.UtcNow;
        await _uow.Users.UpdateAsync(user, ct);
        await _uow.SaveChangesAsync(ct);
        return new UserDto(user.Id, user.Username, user.Email, user.FullName, user.PhoneNumber, user.IsActive, roles, user.CreatedAt);
    }
}

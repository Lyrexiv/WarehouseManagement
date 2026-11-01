using AutoMapper;
using MediatR;
using WarehouseManagement.Application.Features.Users.DTOs;
using WarehouseManagement.Domain.Entities;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.Users.Commands;

public record AssignRoleCommand(AssignRoleDto Dto)
    : IRequest<UserDto>;

public class AssignRoleCommandHandler
    : IRequestHandler<AssignRoleCommand, UserDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public AssignRoleCommandHandler(
        IUnitOfWork uow,
        IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(
        AssignRoleCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _uow.Users.GetWithRolesAsync(
            request.Dto.UserId,
            cancellationToken)
            ?? throw new NotFoundException(
                "User",
                request.Dto.UserId);

        if (user.IsDeleted)
        {
            throw new NotFoundException(
                "User",
                request.Dto.UserId);
        }

        var roleIds = request.Dto.RoleIds
            .Distinct()
            .ToList();

        var roles = new List<Role>();

        foreach (var roleId in roleIds)
        {
            var role = await _uow.Roles.GetByIdAsync(
                roleId,
                cancellationToken)
                ?? throw new NotFoundException("Role", roleId);

            roles.Add(role);
        }

        // Chỉ thay đổi quan hệ User - Role.
        user.UserRoles.Clear();

        foreach (var role in roles)
        {
            user.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id,
                Role = role
            });
        }

        user.UpdatedAt = DateTime.UtcNow;

        await _uow.Users.UpdateAsync(user, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }
}
using AutoMapper;
using MediatR;
using WarehouseManagement.Application.Features.Users.DTOs;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.Users.Commands;

public record UpdateUserCommand(
    Guid Id,
    UpdateUserDto Dto
) : IRequest<UserDto>;

public class UpdateUserCommandHandler
    : IRequestHandler<UpdateUserCommand, UserDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UpdateUserCommandHandler(
        IUnitOfWork uow,
        IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _uow.Users.GetWithRolesAsync(
            request.Id,
            cancellationToken)
            ?? throw new NotFoundException("User", request.Id);

        if (user.IsDeleted)
        {
            throw new NotFoundException("User", request.Id);
        }

        // Không thay đổi Username, PasswordHash và UserRoles.
        _mapper.Map(request.Dto, user);

        user.Email = request.Dto.Email.Trim().ToLowerInvariant();
        user.FullName = request.Dto.FullName.Trim();
        user.PhoneNumber = request.Dto.PhoneNumber?.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        await _uow.Users.UpdateAsync(user, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }
}
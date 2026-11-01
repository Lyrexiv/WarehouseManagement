using AutoMapper;
using FluentValidation;
using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.Users.DTOs;
using WarehouseManagement.Domain.Entities;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.Users.Commands;

public record CreateUserCommand(CreateUserDto Dto)
    : IRequest<UserDto>;

public class CreateUserCommandValidator
    : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Dto.Username)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(50);

        RuleFor(x => x.Dto.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Dto.Password)
            .NotEmpty()
            .MinimumLength(6);

        RuleFor(x => x.Dto.FullName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Dto.RoleIds)
            .NotEmpty()
            .WithMessage("At least one role is required");
    }
}

public class CreateUserCommandHandler
    : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordService _passwordService;
    private readonly IMapper _mapper;

    public CreateUserCommandHandler(
        IUnitOfWork uow,
        IPasswordService passwordService,
        IMapper mapper)
    {
        _uow = uow;
        _passwordService = passwordService;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        var username = request.Dto.Username.Trim();
        var email = request.Dto.Email.Trim().ToLowerInvariant();

        if (await _uow.Users.GetByUsernameAsync(
            username, cancellationToken) != null)
        {
            throw new BusinessException(
                $"Username '{username}' already taken");
        }

        if (await _uow.Users.GetByEmailAsync(
            email, cancellationToken) != null)
        {
            throw new BusinessException(
                $"Email '{email}' already registered");
        }

        // AutoMapper chỉ map các trường được cấu hình.
        var user = _mapper.Map<User>(request.Dto);

        // Chuẩn hóa và xử lý các trường bảo mật thủ công.
        user.Username = username;
        user.Email = email;
        user.FullName = request.Dto.FullName.Trim();
        user.PhoneNumber = request.Dto.PhoneNumber?.Trim();
        user.PasswordHash = _passwordService.HashPassword(
            request.Dto.Password);
        user.IsActive = true;

        var roles = new List<string>();

        foreach (var roleId in request.Dto.RoleIds.Distinct())
        {
            var role = await _uow.Roles.GetByIdAsync(
                roleId,
                cancellationToken)
                ?? throw new NotFoundException("Role", roleId);

            user.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id,
                Role = role
            });

            roles.Add(role.Name);
        }

        await _uow.Users.AddAsync(user, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }
}
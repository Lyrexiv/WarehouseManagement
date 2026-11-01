using AutoMapper;
using MediatR;
using WarehouseManagement.Application.Features.Users.DTOs;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.Users.Queries;

public record GetUserByIdQuery(Guid Id) : IRequest<UserDto>;

public class GetUserByIdQueryHandler
    : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetUserByIdQueryHandler(
        IUnitOfWork uow,
        IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(
        GetUserByIdQuery request,
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

        return _mapper.Map<UserDto>(user);
    }
}
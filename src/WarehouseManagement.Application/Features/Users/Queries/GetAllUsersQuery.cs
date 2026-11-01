using AutoMapper;
using MediatR;
using WarehouseManagement.Application.Common.Models;
using WarehouseManagement.Application.Features.Users.DTOs;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.Users.Queries;

public record GetAllUsersQuery(PaginationParams Params)
    : IRequest<PagedList<UserDto>>;

public class GetAllUsersQueryHandler
    : IRequestHandler<GetAllUsersQuery, PagedList<UserDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetAllUsersQueryHandler(
        IUnitOfWork uow,
        IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PagedList<UserDto>> Handle(
        GetAllUsersQuery request,
        CancellationToken cancellationToken)
    {
        var users = await _uow.Users.GetAllWithRolesAsync(
            cancellationToken);

        var query = users.Where(u => !u.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Params.SearchTerm))
        {
            var searchTerm = request.Params.SearchTerm.Trim();

            query = query.Where(u =>
                u.Username.Contains(
                    searchTerm,
                    StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(
                    searchTerm,
                    StringComparison.OrdinalIgnoreCase));
        }

        var totalCount = query.Count();

        var pagedUsers = query
            .OrderBy(u => u.Username)
            .Skip((request.Params.PageNumber - 1) * request.Params.PageSize)
            .Take(request.Params.PageSize)
            .ToList();

        var userDtos = _mapper.Map<List<UserDto>>(pagedUsers);

        return new PagedList<UserDto>(
            userDtos,
            totalCount,
            request.Params.PageNumber,
            request.Params.PageSize);
    }
}
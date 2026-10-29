using FluentValidation;
using MediatR;
using WarehouseManagement.Application.Common.Models;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.Warehouses;

public record WarehouseDto(Guid Id, string Code, string Name, string Address, string? City, string? Country, decimal Capacity, string? Description, bool IsActive, DateTime CreatedAt);
public record CreateWarehouseDto(string Code, string Name, string Address, string? City, string? Country, decimal Capacity, string? Description);
public record UpdateWarehouseDto(string Code, string Name, string Address, string? City, string? Country, decimal Capacity, string? Description, bool IsActive);

public record GetAllWarehousesQuery(PaginationParams Params) : IRequest<PagedList<WarehouseDto>>;
public record GetWarehouseByIdQuery(Guid Id) : IRequest<WarehouseDto>;
public record CreateWarehouseCommand(CreateWarehouseDto Dto) : IRequest<WarehouseDto>;
public record UpdateWarehouseCommand(Guid Id, UpdateWarehouseDto Dto) : IRequest<WarehouseDto>;
public record DeleteWarehouseCommand(Guid Id) : IRequest<bool>;

public class CreateWarehouseCommandValidator : AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseCommandValidator()
    {
        RuleFor(x => x.Dto.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Dto.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.Address).NotEmpty();
        RuleFor(x => x.Dto.Capacity).GreaterThan(0);
    }
}

public class GetAllWarehousesQueryHandler : IRequestHandler<GetAllWarehousesQuery, PagedList<WarehouseDto>>
{
    private readonly IUnitOfWork _uow;
    public GetAllWarehousesQueryHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<PagedList<WarehouseDto>> Handle(GetAllWarehousesQuery req, CancellationToken ct)
    {
        var all = await _uow.Warehouses.GetAllAsync(ct);
        var q = all.Where(w => !w.IsDeleted).AsQueryable();
        if (!string.IsNullOrEmpty(req.Params.SearchTerm))
        {
            var t = req.Params.SearchTerm.ToLower();
            q = q.Where(w => w.Name.ToLower().Contains(t) || w.Code.ToLower().Contains(t));
        }
        var total = q.Count();
        var items = q.Skip((req.Params.PageNumber - 1) * req.Params.PageSize).Take(req.Params.PageSize)
            .Select(w => new WarehouseDto(w.Id, w.Code, w.Name, w.Address, w.City, w.Country, w.Capacity, w.Description, w.IsActive, w.CreatedAt))
            .ToList();
        return new PagedList<WarehouseDto>(items, total, req.Params.PageNumber, req.Params.PageSize);
    }
}

public class GetWarehouseByIdQueryHandler : IRequestHandler<GetWarehouseByIdQuery, WarehouseDto>
{
    private readonly IUnitOfWork _uow;
    public GetWarehouseByIdQueryHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<WarehouseDto> Handle(GetWarehouseByIdQuery req, CancellationToken ct)
    {
        var w = await _uow.Warehouses.GetByIdAsync(req.Id, ct) ?? throw new NotFoundException("Warehouse", req.Id);
        return new WarehouseDto(w.Id, w.Code, w.Name, w.Address, w.City, w.Country, w.Capacity, w.Description, w.IsActive, w.CreatedAt);
    }
}

public class CreateWarehouseCommandHandler : IRequestHandler<CreateWarehouseCommand, WarehouseDto>
{
    private readonly IUnitOfWork _uow;
    public CreateWarehouseCommandHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<WarehouseDto> Handle(CreateWarehouseCommand req, CancellationToken ct)
    {
        if (await _uow.Warehouses.CodeExistsAsync(req.Dto.Code, null, ct)) throw new DuplicateCodeException("Warehouse", req.Dto.Code);
        var w = new Domain.Entities.Warehouse { Code = req.Dto.Code, Name = req.Dto.Name, Address = req.Dto.Address,
            City = req.Dto.City, Country = req.Dto.Country, Capacity = req.Dto.Capacity, Description = req.Dto.Description };
        await _uow.Warehouses.AddAsync(w, ct);
        await _uow.SaveChangesAsync(ct);
        return new WarehouseDto(w.Id, w.Code, w.Name, w.Address, w.City, w.Country, w.Capacity, w.Description, w.IsActive, w.CreatedAt);
    }
}

public class UpdateWarehouseCommandHandler : IRequestHandler<UpdateWarehouseCommand, WarehouseDto>
{
    private readonly IUnitOfWork _uow;
    public UpdateWarehouseCommandHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<WarehouseDto> Handle(UpdateWarehouseCommand req, CancellationToken ct)
    {
        var w = await _uow.Warehouses.GetByIdAsync(req.Id, ct) ?? throw new NotFoundException("Warehouse", req.Id);
        if (await _uow.Warehouses.CodeExistsAsync(req.Dto.Code, req.Id, ct)) throw new DuplicateCodeException("Warehouse", req.Dto.Code);
        w.Code = req.Dto.Code; w.Name = req.Dto.Name; w.Address = req.Dto.Address;
        w.City = req.Dto.City; w.Country = req.Dto.Country; w.Capacity = req.Dto.Capacity;
        w.Description = req.Dto.Description; w.IsActive = req.Dto.IsActive; w.UpdatedAt = DateTime.UtcNow;
        await _uow.Warehouses.UpdateAsync(w, ct);
        await _uow.SaveChangesAsync(ct);
        return new WarehouseDto(w.Id, w.Code, w.Name, w.Address, w.City, w.Country, w.Capacity, w.Description, w.IsActive, w.CreatedAt);
    }
}

public class DeleteWarehouseCommandHandler : IRequestHandler<DeleteWarehouseCommand, bool>
{
    private readonly IUnitOfWork _uow;
    public DeleteWarehouseCommandHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<bool> Handle(DeleteWarehouseCommand req, CancellationToken ct)
    {
        var w = await _uow.Warehouses.GetByIdAsync(req.Id, ct) ?? throw new NotFoundException("Warehouse", req.Id);
        w.IsDeleted = true; w.UpdatedAt = DateTime.UtcNow;
        await _uow.Warehouses.UpdateAsync(w, ct);
        await _uow.SaveChangesAsync(ct);
        return true;
    }
}

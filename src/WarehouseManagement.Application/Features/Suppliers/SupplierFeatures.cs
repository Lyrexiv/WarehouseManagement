using FluentValidation;
using MediatR;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.Suppliers;

public record SupplierDto(Guid Id, string Code, string Name, string? ContactPerson, string? Email, string? PhoneNumber, string? Address, string? TaxCode, bool IsActive, DateTime CreatedAt);
public record CreateSupplierDto(string Code, string Name, string? ContactPerson, string? Email, string? PhoneNumber, string? Address, string? TaxCode);
public record UpdateSupplierDto(string Code, string Name, string? ContactPerson, string? Email, string? PhoneNumber, string? Address, string? TaxCode, bool IsActive);

public record GetAllSuppliersQuery() : IRequest<IEnumerable<SupplierDto>>;
public record GetSupplierByIdQuery(Guid Id) : IRequest<SupplierDto>;
public record CreateSupplierCommand(CreateSupplierDto Dto) : IRequest<SupplierDto>;
public record UpdateSupplierCommand(Guid Id, UpdateSupplierDto Dto) : IRequest<SupplierDto>;
public record DeleteSupplierCommand(Guid Id) : IRequest<bool>;

public class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierCommandValidator()
    {
        RuleFor(x => x.Dto.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Dto.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Dto.Email));
    }
}

public class GetAllSuppliersQueryHandler : IRequestHandler<GetAllSuppliersQuery, IEnumerable<SupplierDto>>
{
    private readonly IUnitOfWork _uow;
    public GetAllSuppliersQueryHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<IEnumerable<SupplierDto>> Handle(GetAllSuppliersQuery req, CancellationToken ct)
    {
        var all = await _uow.Suppliers.GetAllAsync(ct);
        return all.Where(s => !s.IsDeleted).Select(s => new SupplierDto(s.Id, s.Code, s.Name, s.ContactPerson, s.Email, s.PhoneNumber, s.Address, s.TaxCode, s.IsActive, s.CreatedAt));
    }
}

public class GetSupplierByIdQueryHandler : IRequestHandler<GetSupplierByIdQuery, SupplierDto>
{
    private readonly IUnitOfWork _uow;
    public GetSupplierByIdQueryHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<SupplierDto> Handle(GetSupplierByIdQuery req, CancellationToken ct)
    {
        var s = await _uow.Suppliers.GetByIdAsync(req.Id, ct) ?? throw new NotFoundException("Supplier", req.Id);
        return new SupplierDto(s.Id, s.Code, s.Name, s.ContactPerson, s.Email, s.PhoneNumber, s.Address, s.TaxCode, s.IsActive, s.CreatedAt);
    }
}

public class CreateSupplierCommandHandler : IRequestHandler<CreateSupplierCommand, SupplierDto>
{
    private readonly IUnitOfWork _uow;
    public CreateSupplierCommandHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<SupplierDto> Handle(CreateSupplierCommand req, CancellationToken ct)
    {
        if (await _uow.Suppliers.CodeExistsAsync(req.Dto.Code, null, ct)) throw new DuplicateCodeException("Supplier", req.Dto.Code);
        var s = new Domain.Entities.Supplier { Code = req.Dto.Code, Name = req.Dto.Name, ContactPerson = req.Dto.ContactPerson,
            Email = req.Dto.Email, PhoneNumber = req.Dto.PhoneNumber, Address = req.Dto.Address, TaxCode = req.Dto.TaxCode };
        await _uow.Suppliers.AddAsync(s, ct);
        await _uow.SaveChangesAsync(ct);
        return new SupplierDto(s.Id, s.Code, s.Name, s.ContactPerson, s.Email, s.PhoneNumber, s.Address, s.TaxCode, s.IsActive, s.CreatedAt);
    }
}

public class UpdateSupplierCommandHandler : IRequestHandler<UpdateSupplierCommand, SupplierDto>
{
    private readonly IUnitOfWork _uow;
    public UpdateSupplierCommandHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<SupplierDto> Handle(UpdateSupplierCommand req, CancellationToken ct)
    {
        var s = await _uow.Suppliers.GetByIdAsync(req.Id, ct) ?? throw new NotFoundException("Supplier", req.Id);
        if (await _uow.Suppliers.CodeExistsAsync(req.Dto.Code, req.Id, ct)) throw new DuplicateCodeException("Supplier", req.Dto.Code);
        s.Code = req.Dto.Code; s.Name = req.Dto.Name; s.ContactPerson = req.Dto.ContactPerson;
        s.Email = req.Dto.Email; s.PhoneNumber = req.Dto.PhoneNumber; s.Address = req.Dto.Address;
        s.TaxCode = req.Dto.TaxCode; s.IsActive = req.Dto.IsActive; s.UpdatedAt = DateTime.UtcNow;
        await _uow.Suppliers.UpdateAsync(s, ct);
        await _uow.SaveChangesAsync(ct);
        return new SupplierDto(s.Id, s.Code, s.Name, s.ContactPerson, s.Email, s.PhoneNumber, s.Address, s.TaxCode, s.IsActive, s.CreatedAt);
    }
}

public class DeleteSupplierCommandHandler : IRequestHandler<DeleteSupplierCommand, bool>
{
    private readonly IUnitOfWork _uow;
    public DeleteSupplierCommandHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<bool> Handle(DeleteSupplierCommand req, CancellationToken ct)
    {
        var s = await _uow.Suppliers.GetByIdAsync(req.Id, ct) ?? throw new NotFoundException("Supplier", req.Id);
        s.IsDeleted = true; s.UpdatedAt = DateTime.UtcNow;
        await _uow.Suppliers.UpdateAsync(s, ct);
        await _uow.SaveChangesAsync(ct);
        return true;
    }
}

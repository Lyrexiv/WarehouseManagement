using MediatR;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.Users.Commands;

public record DeleteUserCommand(Guid Id) : IRequest<bool>;

public class DeleteUserCommandHandler
    : IRequestHandler<DeleteUserCommand, bool>
{
    private readonly IUnitOfWork _uow;

    public DeleteUserCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(
        DeleteUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _uow.Users.GetByIdAsync(
            request.Id,
            cancellationToken)
            ?? throw new NotFoundException("User", request.Id);

        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _uow.Users.UpdateAsync(user, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return true;
    }
}
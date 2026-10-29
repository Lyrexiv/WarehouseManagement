using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.Entities;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Infrastructure.Persistence.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext ctx)
        : base(ctx)
    {
    }

    public async Task<User?> GetByUsernameAsync(
        string username,
        CancellationToken ct = default)
        => await _db
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Username == username, ct);

    public async Task<User?> GetByEmailAsync(
        string email,
        CancellationToken ct = default)
        => await _db
            .FirstOrDefaultAsync(x => x.Email == email, ct);

    public async Task<User?> GetByRefreshTokenHashAsync(
        string refreshTokenHash,
        CancellationToken ct = default)
        => await _db
            .FirstOrDefaultAsync(
                x => x.RefreshTokenHash == refreshTokenHash,
                ct);

    public async Task<User?> GetWithRolesAsync(
        Guid id,
        CancellationToken ct = default)
        => await _db
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<User>> GetAllWithRolesAsync(
        CancellationToken ct = default)
        => await _db
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .ToListAsync(ct);
}
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _ctx;
    protected readonly DbSet<T> _db;

    public Repository(ApplicationDbContext ctx) { _ctx = ctx; _db = ctx.Set<T>(); }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.FirstOrDefaultAsync(e => e.Id == id, ct);
    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
        => await _db.ToListAsync(ct);
    public virtual async Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> pred, CancellationToken ct = default)
        => await _db.Where(pred).ToListAsync(ct);
    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> pred, CancellationToken ct = default)
        => await _db.FirstOrDefaultAsync(pred, ct);
    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> pred, CancellationToken ct = default)
        => await _db.AnyAsync(pred, ct);
    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? pred = null, CancellationToken ct = default)
        => pred == null ? await _db.CountAsync(ct) : await _db.CountAsync(pred, ct);
    public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await _db.AddAsync(entity, ct);
        return entity;
    }
    public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
    {
        var list = entities.ToList();
        await _db.AddRangeAsync(list, ct);
        return list;
    }
    public virtual Task UpdateAsync(T entity, CancellationToken ct = default) { _db.Update(entity); return Task.CompletedTask; }
    public virtual Task DeleteAsync(T entity, CancellationToken ct = default) { _db.Remove(entity); return Task.CompletedTask; }
    public virtual IQueryable<T> Query() => _db.AsQueryable();
}

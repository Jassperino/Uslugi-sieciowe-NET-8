using BlogCMS.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogCMS.Repositories;

public class EfCoreRepository<T> : IRepository<T> where T : class
{
    private readonly BlogDbContext _context;
    private readonly DbSet<T> _entities;

    public EfCoreRepository(BlogDbContext context)
    {
        _context = context;
        _entities = context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _entities
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<T?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _entities.FindAsync([id], cancellationToken);
    }

    public async Task<int> AddAsync(
        T entity,
        CancellationToken cancellationToken = default)
    {
        await _entities.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var idProperty = _context.Entry(entity).Property("Id");
        return (int)(idProperty.CurrentValue
            ?? throw new InvalidOperationException("Entity does not have a generated Id."));
    }

    public async Task<bool> UpdateAsync(
        T entity,
        CancellationToken cancellationToken = default)
    {
        _entities.Update(entity);
        return await _context.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _entities.FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return false;
        }

        _entities.Remove(entity);
        return await _context.SaveChangesAsync(cancellationToken) > 0;
    }
}

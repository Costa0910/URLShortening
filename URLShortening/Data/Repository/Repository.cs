using Microsoft.EntityFrameworkCore;

namespace URLShortening.Data.Repository;

public class Repository<T>(DataContext context): IRepository<T>  where T:
    class , IEntity
{
    public async Task<T?> GetByIdAsync(string id)
        => await context.Set<T>().FindAsync(id);

    public async Task<IEnumerable<T>> GetAllAsync()
        => await context.Set<T>().ToListAsync();

    public async Task AddAsync(T entity)
    {
        await context.Set<T>().AddAsync(entity);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        context.Set<T>().Update(entity);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        context.Set<T>().Remove(entity);
        await context.SaveChangesAsync();
    }
}

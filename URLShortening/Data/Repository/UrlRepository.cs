using Microsoft.EntityFrameworkCore;
using URLShortening.Data;

namespace URLShortening.Data.Repository;

public class UrlRepository(DataContext context) : Repository<Url>(context),
    IUrlRepository
{
    public async Task<Url?> FindByShortUrl(string shortUrl)
    {
        //TODO: check way to improve performance
        return await context.Urls
            .Include(u => u.AccessLogs)
            .FirstOrDefaultAsync(
                u => u.ShortId == shortUrl);
    }

    public async Task<Url?> FindByLongUrl(string lonUrl)
    {
        return await context.Urls
            .Include(u => u.AccessLogs)
            .FirstOrDefaultAsync(
                u => u.LongUrl == lonUrl);
    }

    public async Task<IEnumerable<Url>> GetTopUrlsAsync()
    {
        return await context.Urls
            .Include(u => u.AccessLogs)
            .OrderByDescending(u => u.AccessLogs.Count)
            .Take(5)
            .ToListAsync();
    }
}

namespace URLShortening.Data.Repository;

public interface IUrlRepository : IRepository<Url>
{
    Task<Url?> FindByShortUrl(string shortUrl);
    Task<Url?> FindByLongUrl(string lonUrl);
    Task<IEnumerable<Url>> GetTopUrlsAsync();
}

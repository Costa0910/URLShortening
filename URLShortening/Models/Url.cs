namespace URLShortening.Models;

public class Url
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ShortId { get; set; }
    public string LongUrl { get; set; }
    public string? CustomAlias { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<AccessLog> AccessLogs { get; set; } = [];
}
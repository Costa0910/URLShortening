namespace URLShortening.Data;

public class Url : IEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ShortId { get; set; }
    public string LongUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<AccessLog> AccessLogs { get; set; } = [];
}

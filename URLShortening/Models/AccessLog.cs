namespace URLShortening.Models;

public class AccessLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime AccessedAt { get; set; }
    public string IPAddress { get; set; }
    public string UserAgent { get; set; }
    public string Location { get; set; }
    public string Ref { get; set; }
}
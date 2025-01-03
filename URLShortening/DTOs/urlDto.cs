namespace URLShortening.DTOs;

public class urlDto
{
    public string Id { get; set; }
    public string Url { get; set; }
    public string ShortCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

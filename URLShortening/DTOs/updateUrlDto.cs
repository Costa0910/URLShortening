using System.ComponentModel.DataAnnotations;

namespace URLShortening.DTOs;

public class updateUrlDto
{
    [Required, StringLength(8)] public string ShortCode { get; set; }
    [Required] public DateTime ExpiresAt { get; set; }
}

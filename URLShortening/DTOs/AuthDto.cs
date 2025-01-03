using System.ComponentModel.DataAnnotations;

namespace URLShortening.DTOs;

public class AuthDto
{
    [EmailAddress, Required] public string Email { get; set; }
    [Required, MinLength(8)] public string Password { get; set; }
}

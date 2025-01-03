using System.ComponentModel.DataAnnotations;

namespace URLShortening.DTOs;

public class EmailConfirmationDto
{
    [Required, MaxLength(10)] public string ConfirmationCode { get; set; }
    [Required, EmailAddress] public string Email { set; get; }
}

using System.Security.Policy;
using Microsoft.AspNetCore.Identity;

namespace URLShortening.Models;

public class User : IdentityUser
{
    public List<Url> Urls { get; set; } = [];
}

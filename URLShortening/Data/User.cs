using System.Security.Policy;
using Microsoft.AspNetCore.Identity;

namespace URLShortening.Data;

public class User : IdentityUser
{
    public List<Url> Urls { get; set; } = [];
}

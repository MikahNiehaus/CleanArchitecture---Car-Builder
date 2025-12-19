using Microsoft.AspNetCore.Identity;

namespace CarBuilder.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

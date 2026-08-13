using Microsoft.AspNetCore.Identity;

namespace CustomerSupport.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public Guid CustomerId { get; set; }
}

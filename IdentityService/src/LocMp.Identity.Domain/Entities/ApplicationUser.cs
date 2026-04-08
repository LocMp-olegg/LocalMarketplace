using Microsoft.AspNetCore.Identity;

namespace LocMp.Identity.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public int? Gender { get; set; }
    public DateTime? BirthDate { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public bool Active { get; set; } = true;
    
    public virtual ICollection<UserAddress> Addresses { get; set; } = new List<UserAddress>();
    public virtual UserPhoto? Photo { get; set; }
}
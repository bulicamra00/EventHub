using EventHub.Domain.Common;
using EventHub.Domain.Enums;

namespace EventHub.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string City { get; set; } = string.Empty;

    public bool IsEmailVerified { get; set; } = false;
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiry { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public DateTime? RefreshTokenCreated { get; set; }
    public DateTime? RefreshTokenRevoked { get; set; }
    public string? ReplacedByToken { get; set; } 
    
    public ICollection<User> Following { get; set; } = new List<User>();
    public ICollection<Event> OrganizedEvents { get; set; } = new List<Event>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public string? Interests { get; set; } 

    public bool IsBlocked { get; set; } = false;
    public string? BanReason { get; set; }

    public bool IsOrganizerRequested { get; set; } = false;
    public string? OrganizerRequestStatus { get; set; } 
}
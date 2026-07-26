using EventHub.Domain.Common;

namespace EventHub.Domain.Entities;

public class EventInvitation : BaseEntity
{
    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;
    
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = Guid.NewGuid().ToString();
    
    public bool IsUsed { get; private set; } = false;

    public void Accept()
    {
        if (IsUsed)
            throw new InvalidOperationException("Ova pozivnica je već iskorišćena.");
        
        IsUsed = true;
    }
}
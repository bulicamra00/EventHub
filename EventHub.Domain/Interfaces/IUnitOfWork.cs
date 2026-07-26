using EventHub.Domain.Entities;

namespace EventHub.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<User> Users { get; }
    IGenericRepository<Event> Events { get; }
    IGenericRepository<EventSeries> EventSeries { get; }
    IGenericRepository<Category> Categories { get; }
    IGenericRepository<TicketType> TicketTypes { get; }
    IGenericRepository<Ticket> Tickets { get; }
    IGenericRepository<Tag> Tags { get; }
    IGenericRepository<Booking> Bookings { get; }
    IGenericRepository<Review> Reviews { get; }
    IGenericRepository<EventInvitation> EventInvitations { get; } 
    IGenericRepository<EventTag> EventTags { get; }
    IGenericRepository<Follow> Follows { get; }
    
    IGenericRepository<Notification> Notifications { get; }
    
    Task<int> CompleteAsync();
}
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using EventHub.Infrastructure.Persistence.Repositories;

namespace EventHub.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    
    private IGenericRepository<User>? _users;
    private IGenericRepository<Event>? _events;
    private IGenericRepository<EventSeries>? _eventSeries;
    private IGenericRepository<Category>? _categories;
    private IGenericRepository<TicketType>? _ticketTypes;
    private IGenericRepository<Ticket>? _tickets;
    private IGenericRepository<Tag>? _tags;
    private IGenericRepository<EventTag>? _eventTags; 
    private IGenericRepository<Booking>? _bookings;
    private IGenericRepository<Review>? _reviews;
    private IGenericRepository<EventInvitation>? _eventInvitations;
    private IGenericRepository<Follow>? _follows;
    private IGenericRepository<Notification>? _notifications;

    public UnitOfWork(ApplicationDbContext context) => _context = context;

    public IGenericRepository<User> Users => _users ??= new GenericRepository<User>(_context);
    public IGenericRepository<Event> Events => _events ??= new GenericRepository<Event>(_context);
    public IGenericRepository<EventSeries> EventSeries => _eventSeries ??= new GenericRepository<EventSeries>(_context);
    public IGenericRepository<Category> Categories => _categories ??= new GenericRepository<Category>(_context);
    public IGenericRepository<TicketType> TicketTypes => _ticketTypes ??= new GenericRepository<TicketType>(_context);
    public IGenericRepository<Ticket> Tickets => _tickets ??= new GenericRepository<Ticket>(_context);
    public IGenericRepository<Tag> Tags => _tags ??= new GenericRepository<Tag>(_context);
    
    public IGenericRepository<EventTag> EventTags => _eventTags ??= new GenericRepository<EventTag>(_context);
    
    public IGenericRepository<Booking> Bookings => _bookings ??= new GenericRepository<Booking>(_context);
    public IGenericRepository<Review> Reviews => _reviews ??= new GenericRepository<Review>(_context);
    public IGenericRepository<EventInvitation> EventInvitations => _eventInvitations ??= new GenericRepository<EventInvitation>(_context);
    public IGenericRepository<Follow> Follows => _follows ??= new GenericRepository<Follow>(_context);
    public IGenericRepository<Notification> Notifications => _notifications ??= new GenericRepository<Notification>(_context);

    public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();

    public void Dispose() => _context.Dispose();
}
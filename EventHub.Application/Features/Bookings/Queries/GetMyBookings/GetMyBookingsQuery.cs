using MediatR;

namespace EventHub.Application.Features.Bookings.Queries.GetMyBookings;

public record GetMyBookingsQuery(string UserId) : IRequest<List<BookingDto>>;
using MediatR;
using EventHub.Application.Features.Bookings.Queries.GetMyBookings; // OVO TI FALI

namespace EventHub.Application.Features.Bookings.Queries.GetBookingById;

public record GetBookingByIdQuery(Guid Id) : IRequest<BookingDto>;
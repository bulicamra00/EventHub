using MediatR;

namespace EventHub.Application.Features.Bookings.Commands.CancelExpiredBookings;

public record CancelExpiredBookingsCommand : IRequest;
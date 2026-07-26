using FluentValidation;

namespace EventHub.Application.Features.Bookings.Commands.CancelExpiredBookings;

public class CancelExpiredBookingsCommandValidator : AbstractValidator<CancelExpiredBookingsCommand>
{
    public CancelExpiredBookingsCommandValidator()
    {
    }
}
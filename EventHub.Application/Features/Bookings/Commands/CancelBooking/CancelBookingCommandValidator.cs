using EventHub.Domain.Interfaces;
using FluentValidation;

namespace EventHub.Application.Features.Bookings.Commands.CancelBooking;

public class CancelBookingCommandValidator : AbstractValidator<CancelBookingCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CancelBookingCommandValidator(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;

        RuleFor(x => x.BookingId)
            .NotEmpty().WithMessage("ID rezervacije je obavezan.");

        RuleFor(x => x)
            .MustAsync(BeOwner).WithMessage("Nemate pravo da otkažete ovu rezervaciju jer niste njen vlasnik.");
    }

    private async Task<bool> BeOwner(CancelBookingCommand command, CancellationToken ct)
    {
        var booking = await _unitOfWork.Bookings.GetByIdAsync(command.BookingId);
        if (booking == null) return false;

        return booking.UserId == _currentUserService.UserId;
    }
}
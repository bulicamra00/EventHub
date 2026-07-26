using EventHub.Domain.Interfaces;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;

namespace EventHub.Infrastructure.Services;

public class PaymentProcessingService : IPaymentProcessingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IQrCodeService _qrCodeService;
    private readonly IPaymentService _paymentService;

    public PaymentProcessingService(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IQrCodeService qrCodeService,
        IPaymentService paymentService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _qrCodeService = qrCodeService;
        _paymentService = paymentService;
    }

    public async Task ProcessAsync(Guid ticketId)
    {
        var ticket = await _unitOfWork.Tickets.GetByIdAsync(ticketId);
        if (ticket == null) return;

        var paymentSuccess = await _paymentService.ProcessPaymentAsync(ticket.UserId, ticket.PurchasePrice, "tok_mock_123");

        if (paymentSuccess)
        {
            ticket.Status = TicketStatus.Active;

            var pendingBookings = await _unitOfWork.Bookings.GetListByConditionAsync(
                b => b.UserId == ticket.UserId && b.EventId == ticket.EventId && b.Status == BookingStatus.Pending
            );
            
            var booking = pendingBookings.FirstOrDefault();
            if (booking != null)
            {
                booking.Status = BookingStatus.Confirmed; 
            }

            await _unitOfWork.CompleteAsync();

            var qrCodeBase64 = _qrCodeService.GenerateQrCode(ticket.TicketCode);
            var subject = "Potvrda o uspešnoj kupovini karte";
            var body = $@"<h1>Hvala na kupovini!</h1>
                          <p>Vaš kod: <strong>{ticket.TicketCode}</strong></p>
                          <img src='data:image/png;base64,{qrCodeBase64}' />";

            await _emailService.SendEmailAsync(ticket.AttendeeEmail, subject, body);
        }
    }
}
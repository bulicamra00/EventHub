namespace EventHub.Domain.Interfaces;

public interface IPaymentProcessingService
{
    Task ProcessAsync(Guid ticketId);
}
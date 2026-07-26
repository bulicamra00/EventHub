namespace EventHub.Domain.Interfaces;

public interface IJobService
{
    void EnqueuePaymentProcessing(Guid ticketId);
}
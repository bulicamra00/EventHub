using EventHub.Domain.Interfaces;
using Hangfire;

namespace EventHub.Infrastructure.Services;

public class JobService : IJobService
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public JobService(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public void EnqueuePaymentProcessing(Guid ticketId)
    {
        _backgroundJobClient.Enqueue<IPaymentProcessingService>(x => x.ProcessAsync(ticketId));
    }
}
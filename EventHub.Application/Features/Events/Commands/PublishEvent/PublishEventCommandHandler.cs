using EventHub.Application.Features.Events.Commands.PublishEvent;
using EventHub.Domain.Interfaces;
using EventHub.Domain.Enums;
using MediatR;

namespace EventHub.Application.Features.Events.Commands.PublishEvent;

public class PublishEventCommandHandler : IRequestHandler<PublishEventCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public PublishEventCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(PublishEventCommand request, CancellationToken ct)
    {
        var eventEntity = await _unitOfWork.Events.GetByIdAsync(request.EventId);

        if (eventEntity == null)
            return false;

        if (eventEntity.Status == EventStatus.Published)
            return true;

        try 
        {
            eventEntity.Publish();
        }
        catch (InvalidOperationException)
        {
            return false;
        }

        _unitOfWork.Events.Update(eventEntity);
        await _unitOfWork.CompleteAsync();

        return true;
    }
}
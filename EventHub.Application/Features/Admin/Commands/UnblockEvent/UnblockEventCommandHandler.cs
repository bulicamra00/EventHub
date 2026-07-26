using EventHub.Application.Exceptions;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using MediatR;

namespace EventHub.Application.Features.Admin.Commands.UnblockEvent;

public class UnblockEventCommandHandler : IRequestHandler<UnblockEventCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UnblockEventCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task Handle(UnblockEventCommand request, CancellationToken cancellationToken)
    {
        var eventEntity = await _unitOfWork.Events.GetByIdAsync(request.EventId);

        if (eventEntity == null)
            throw new NotFoundException(nameof(Event), request.EventId);

        eventEntity.Unblock();

        await _unitOfWork.CompleteAsync();
    }
}
using EventHub.Application.Exceptions;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using MediatR;

namespace EventHub.Application.Features.Admin.Commands.BlockEvent;

public class BlockEventCommandHandler : IRequestHandler<BlockEventCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public BlockEventCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task Handle(BlockEventCommand request, CancellationToken cancellationToken)
    {
        var eventEntity = await _unitOfWork.Events.GetByIdAsync(request.EventId);

        if (eventEntity == null)
            throw new NotFoundException(nameof(Event), request.EventId);

        eventEntity.Block(request.Reason);

        await _unitOfWork.CompleteAsync();
    }
}
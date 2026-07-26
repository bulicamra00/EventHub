using EventHub.Application.Features.Tickets.Commands.CreateTicketType;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using MediatR;

namespace EventHub.Application.Features.Tickets.Commands.CreateTicketType;

public class CreateTicketTypeCommandHandler : IRequestHandler<CreateTicketTypeCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateTicketTypeCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateTicketTypeCommand request, CancellationToken cancellationToken)
    {
        var ticketType = new TicketType(request.Name, request.Capacity, request.Price)
        {
            EventId = request.EventId,
            EarlyBirdPrice = request.EarlyBirdPrice,
            EarlyBirdExpiryDate = request.EarlyBirdExpiryDate
        };

        await _unitOfWork.TicketTypes.AddAsync(ticketType);
        await _unitOfWork.CompleteAsync(); 

        return ticketType.Id;
    }
}
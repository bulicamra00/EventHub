using AutoMapper;
using EventHub.Application.Features.Tickets.Queries.GetMyTickets;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;

namespace EventHub.Application.Mappings;

public class TicketQrCodeResolver : IValueResolver<Ticket, TicketDto, string>
{
    private readonly IQrCodeService _qrCodeService;

    public TicketQrCodeResolver(IQrCodeService qrCodeService)
    {
        _qrCodeService = qrCodeService;
    }

    public string Resolve(Ticket source, TicketDto destination, string destMember, ResolutionContext context)
    {
        return _qrCodeService.GenerateQrCode(source.TicketCode.ToString());
    }
}
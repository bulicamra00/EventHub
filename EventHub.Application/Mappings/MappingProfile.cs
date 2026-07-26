using AutoMapper;
using EventHub.Domain.Entities;
using EventHub.Application.Features.Events.Queries.GetEvents;
using EventHub.Application.Features.Bookings.Queries.GetMyBookings;
using EventHub.Application.Features.Tickets.Queries.GetMyTickets;
using EventHub.Application.Features.Events.Queries.GetEventDetails;
using EventHub.Application.Features.Reviews.Queries.GetEventReviews;
using EventHub.Domain.Enums;
using System.Linq;

namespace EventHub.Application.Mappings;

public class MappingProfile : Profile 
{
    public MappingProfile() 
    {
        CreateMap<Event, EventDto>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category != null ? s.Category.Name : string.Empty))
            .ForMember(d => d.TagNames, o => o.MapFrom(s => s.EventTags.Select(et => et.Tag.Name).ToList()))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status))
            .ForMember(d => d.IsBlocked, o => o.MapFrom(s => s.IsBlocked))
            .ForMember(d => d.BlockReason, o => o.MapFrom(s => s.BlockReason));

        CreateMap<EventInvitation, EventDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Event.Id))
            .ForMember(d => d.Title, o => o.MapFrom(s => s.Event.Title))
            .ForMember(d => d.Description, o => o.MapFrom(s => s.Event.Description))
            .ForMember(d => d.StartDate, o => o.MapFrom(s => s.Event.StartDate))
            .ForMember(d => d.EndDate, o => o.MapFrom(s => s.Event.EndDate))
            .ForMember(d => d.Location, o => o.MapFrom(s => s.Event.Location))
            .ForMember(d => d.CoverImageUrl, o => o.MapFrom(s => s.Event.CoverImageUrl))
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Event.Category != null ? s.Event.Category.Name : string.Empty))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Event.Status))
            .ForMember(d => d.TagNames, o => o.Ignore())
            .ForMember(d => d.IsBlocked, o => o.MapFrom(s => s.Event.IsBlocked))
            .ForMember(d => d.BlockReason, o => o.MapFrom(s => s.Event.BlockReason));

        CreateMap<Event, EventDetailsDto>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category != null ? s.Category.Name : string.Empty))
            .ForMember(d => d.TagNames, o => o.MapFrom(s => s.EventTags.Select(et => et.Tag.Name).ToList()))
            .ForMember(d => d.IsBookable, o => o.MapFrom(s => s.IsSaleActive()))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status))
            .ForMember(d => d.CancelReason, o => o.MapFrom(s => s.CancelReason));

        CreateMap<TicketType, TicketTypeDto>()
            .ForMember(d => d.AvailableCapacity, o => o.MapFrom(s => s.Capacity - s.SoldCount - s.ReservedCount));

        CreateMap<Booking, BookingDto>()
            .ForMember(d => d.EventTitle, o => o.MapFrom(s => s.Event != null ? s.Event.Title : string.Empty))
            .ForMember(d => d.TicketTypeId, o => o.MapFrom(s => s.TicketTypeId));

        CreateMap<Ticket, TicketDto>()
            .ForMember(d => d.EventName, o => o.MapFrom(s => s.Event != null ? s.Event.Title : string.Empty))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.QrCodeBase64, o => o.MapFrom<TicketQrCodeResolver>())
            .ForMember(d => d.PurchasePrice, o => o.MapFrom(s => s.PurchasePrice))
            .ForMember(d => d.PurchaseDate, o => o.MapFrom(s => s.PurchaseDate));

        CreateMap<Review, ReviewDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User != null ? s.User.FullName : "Anonimni korisnik"))
            .ForMember(d => d.Rating, o => o.MapFrom(s => s.Rating))
            .ForMember(d => d.Comment, o => o.MapFrom(s => s.Comment))
            .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.CreatedAt));
    }
}
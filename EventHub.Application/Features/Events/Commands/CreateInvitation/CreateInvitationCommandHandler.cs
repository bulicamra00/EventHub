using MediatR;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace EventHub.Application.Features.Events.Commands.CreateInvitation;

public class CreateInvitationCommandHandler : IRequestHandler<CreateInvitationCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public CreateInvitationCommandHandler(IUnitOfWork uow, IEmailService emailService)
    {
        _unitOfWork = uow;
        _emailService = emailService;
    }

    public async Task<Guid> Handle(CreateInvitationCommand request, CancellationToken ct)
    {
        var existingEvent = await _unitOfWork.Events.GetByIdAsync(request.EventId);
        if (existingEvent == null)
        {
            throw new Exception($"Event sa ID-jem {request.EventId} nije pronađen.");
        }

        var invitation = new EventInvitation 
        { 
            EventId = request.EventId, 
            Email = request.Email,
            Token = Guid.NewGuid().ToString()
        };

        await _unitOfWork.EventInvitations.AddAsync(invitation);
        await _unitOfWork.CompleteAsync();

        var invitationLink = $"http://localhost:5173/events/{invitation.EventId}?token={invitation.Token}";
        
        var emailBody = $@"
            <div style='font-family: Arial, sans-serif; line-height: 1.6;'>
                <h2>Pozivnica za događaj</h2>
                <p>Dragi prijatelju,</p>
                <p>Pozivamo te na događaj! Klikni na link ispod da vidiš detalje i potvrdiš dolazak:</p>
                <p>
                    <a href='{invitationLink}' 
                       style='background-color: #2563eb; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold;'>
                       Pogledaj događaj
                    </a>
                </p>
                <p style='color: #666; font-size: 0.9em;'>
                    Ako dugme ne radi, kopiraj ovaj link u svoj pretraživač:<br/>
                    {invitationLink}
                </p>
            </div>";

        await _emailService.SendEmailAsync(
            invitation.Email, 
            "Pozivnica za događaj", 
            emailBody
        );

        return invitation.Id;
    }
}
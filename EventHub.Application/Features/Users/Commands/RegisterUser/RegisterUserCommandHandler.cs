using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using EventHub.Domain.Enums;
using MediatR;
using BCrypt.Net;

namespace EventHub.Application.Features.Users.Commands.RegisterUser;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IAppConfig _appConfig; 

    public RegisterUserCommandHandler(IUnitOfWork unitOfWork, IEmailService emailService, IAppConfig appConfig)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _appConfig = appConfig;
    }

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _unitOfWork.Users.GetByConditionAsync(u => u.Email == request.Email);
        if (existingUser != null)
        {
            throw new Exception("Korisnik sa ovom email adresom već postoji.");
        }

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            City = request.City,
            Role = UserRole.Attendee,
            EmailVerificationToken = Guid.NewGuid().ToString(),
            EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24)
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.CompleteAsync();

        var verificationUrl = $"{_appConfig.FrontendUrl}/verify-email?token={user.EmailVerificationToken}";
        
        await _emailService.SendEmailAsync(user.Email, "Dobrodošli u EventHub - Potvrdite nalog", 
            $"Kliknite na link da potvrdite nalog: <a href='{verificationUrl}'>Potvrdi email</a>");

        return user.Id;
    }
}
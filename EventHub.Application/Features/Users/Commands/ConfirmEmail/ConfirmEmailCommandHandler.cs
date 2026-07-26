using EventHub.Application.Features.Users.Commands.ConfirmEmail;
using EventHub.Domain.Interfaces;
using MediatR;

namespace EventHub.Application.Features.Users.Commands.ConfirmEmail;

public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public ConfirmEmailCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByConditionAsync(u => u.EmailVerificationToken == request.Token);

        if (user == null || user.EmailVerificationTokenExpiry < DateTime.UtcNow)
        {
            return false;
        }

        user.IsEmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiry = null;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.CompleteAsync();

        return true;
    }
}
using FluentValidation;

namespace EventHub.Application.Features.Reviews.Commands.CreateReview;

public class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(c => c.EventId).NotEmpty();
        RuleFor(c => c.Rating).InclusiveBetween(1, 5).WithMessage("Ocena mora biti između 1 i 5.");
        RuleFor(c => c.Comment).MaximumLength(500).WithMessage("Komentar ne može biti duži od 500 karaktera.");
    }
}
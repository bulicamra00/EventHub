using EventHub.Application.Features.Reviews.Commands.CreateReview;
using FluentValidation.TestHelper;
using System;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Reviews.Commands.CreateReview;

public class CreateReviewCommandValidatorTests
{
    private readonly CreateReviewCommandValidator _validator;

    public CreateReviewCommandValidatorTests()
    {
        _validator = new CreateReviewCommandValidator();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenEventIdIsEmpty()
    {
        var command = new CreateReviewCommand(Guid.Empty, 4, "Dobar događaj");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.EventId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Validate_ShouldHaveError_WhenRatingIsOutOfRange(int invalidRating)
    {
        var command = new CreateReviewCommand(Guid.NewGuid(), invalidRating, "Komentar");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Rating)
              .WithErrorMessage("Ocena mora biti između 1 i 5.");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void Validate_ShouldNotHaveError_WhenRatingIsValid(int validRating)
    {
        var command = new CreateReviewCommand(Guid.NewGuid(), validRating, "Komentar");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.Rating);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenCommentExceedsMaxLength()
    {
        var longComment = new string('a', 501);
        var command = new CreateReviewCommand(Guid.NewGuid(), 5, longComment);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Comment)
              .WithErrorMessage("Komentar ne može biti duži od 500 karaktera.");
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenCommentIsEmpty()
    {
        var command = new CreateReviewCommand(Guid.NewGuid(), 5, string.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.Comment);
    }
}
using AutoMapper;
using EventHub.Application.Features.Reviews.Queries.GetEventReviews;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Reviews.Queries.GetEventReviews;

public class GetEventReviewsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IGenericRepository<Review>> _reviewsRepoMock;

    private readonly GetEventReviewsQueryHandler _handler;

    public GetEventReviewsQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _reviewsRepoMock = new Mock<IGenericRepository<Review>>();

        _unitOfWorkMock.Setup(u => u.Reviews).Returns(_reviewsRepoMock.Object);

        _handler = new GetEventReviewsQueryHandler(
            _unitOfWorkMock.Object,
            _mapperMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoReviewsExistForEvent()
    {
        var eventId = Guid.NewGuid();
        _reviewsRepoMock
            .Setup(r => r.GetListByConditionAsync(
                It.IsAny<Expression<Func<Review, bool>>>(),
                It.Is<string>(s => s == "User")))
            .ReturnsAsync(new List<Review>());

        var query = new GetEventReviewsQuery(eventId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnReviewsOrderedByCreatedAtDescending_WhenReviewsExist()
    {
        var eventId = Guid.NewGuid();
        var olderDate = DateTime.UtcNow.AddDays(-2);
        var newerDate = DateTime.UtcNow.AddDays(-1);

        var olderReview = new Review
        {
            EventId = eventId,
            Rating = 4,
            Comment = "Starija recenzija",
            User = new User { FullName = "Ana Anic" }
        };
        SetProperty(olderReview, "Id", Guid.NewGuid());
        SetProperty(olderReview, "CreatedAt", olderDate);

        var newerReview = new Review
        {
            EventId = eventId,
            Rating = 5,
            Comment = "Novija recenzija",
            User = new User { FullName = "Marko Markovic" }
        };
        SetProperty(newerReview, "Id", Guid.NewGuid());
        SetProperty(newerReview, "CreatedAt", newerDate);

        var reviewsList = new List<Review> { olderReview, newerReview };

        _reviewsRepoMock
            .Setup(r => r.GetListByConditionAsync(
                It.IsAny<Expression<Func<Review, bool>>>(),
                It.Is<string>(s => s == "User")))
            .ReturnsAsync(reviewsList);

        var query = new GetEventReviewsQuery(eventId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        result[0].Id.Should().Be(newerReview.Id);
        result[0].UserName.Should().Be("Marko Markovic");
        result[0].Rating.Should().Be(5);
        result[0].Comment.Should().Be("Novija recenzija");
        result[0].CreatedAt.Should().Be(newerDate);

        result[1].Id.Should().Be(olderReview.Id);
        result[1].UserName.Should().Be("Ana Anic");
        result[1].Rating.Should().Be(4);
        result[1].Comment.Should().Be("Starija recenzija");
        result[1].CreatedAt.Should().Be(olderDate);
    }

    [Fact]
    public async Task Handle_ShouldReturnAnonymousUser_WhenReviewUserIsNull()
    {
        var eventId = Guid.NewGuid();
        var reviewDate = DateTime.UtcNow;

        var reviewWithoutUser = new Review
        {
            EventId = eventId,
            Rating = 3,
            Comment = "Prosečno",
            User = null!
        };
        SetProperty(reviewWithoutUser, "Id", Guid.NewGuid());
        SetProperty(reviewWithoutUser, "CreatedAt", reviewDate);

        _reviewsRepoMock
            .Setup(r => r.GetListByConditionAsync(
                It.IsAny<Expression<Func<Review, bool>>>(),
                It.Is<string>(s => s == "User")))
            .ReturnsAsync(new List<Review> { reviewWithoutUser });

        var query = new GetEventReviewsQuery(eventId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].UserName.Should().Be("Anonimni korisnik");
    }

    private void SetProperty(object obj, string propertyName, object value)
    {
        var property = obj.GetType().GetProperty(propertyName) ?? obj.GetType().BaseType?.GetProperty(propertyName);
        property?.SetValue(obj, value);
    }
}
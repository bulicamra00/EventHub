using AutoMapper;
using EventHub.Application.Features.Events.Queries.GetEvents;
using EventHub.Application.Features.Events.Queries.GetMyInvitations;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Events.Queries.GetMyInvitations;

public class GetMyInvitationsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IGenericRepository<EventInvitation>> _invitationRepoMock;

    private readonly GetMyInvitationsQueryHandler _handler;

    public GetMyInvitationsQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _invitationRepoMock = new Mock<IGenericRepository<EventInvitation>>();

        _unitOfWorkMock.Setup(u => u.EventInvitations).Returns(_invitationRepoMock.Object);

        _handler = new GetMyInvitationsQueryHandler(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _currentUserServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserEmailIsNullOrEmpty()
    {
        _currentUserServiceMock.Setup(s => s.Email).Returns(string.Empty);
        var query = new GetMyInvitationsQuery();

        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
                 .WithMessage("Korisnik nije ulogovan.");
    }

    [Fact]
    public async Task Handle_ShouldReturnListOfEventDtos_WhenInvitationsExist()
    {
        var userEmail = "test@example.com";
        _currentUserServiceMock.Setup(s => s.Email).Returns(userEmail);

        var invitationsList = new List<EventInvitation>
        {
            new EventInvitation
            {
                Email = userEmail,
                Event = new Event
                {
                    Title = "Invited Event",
                    Description = "Description",
                    Location = "Belgrade"
                }
            }
        };

        _invitationRepoMock.Setup(r => r.GetListByConditionAsync(
                               It.IsAny<Expression<Func<EventInvitation, bool>>>(),
                               It.IsAny<string[]>()))
                           .ReturnsAsync(invitationsList);

        var expectedDtos = new List<EventDto>
        {
            new EventDto { Title = "Invited Event" }
        };

        _mapperMock.Setup(m => m.Map<List<EventDto>>(invitationsList))
                   .Returns(expectedDtos);

        var query = new GetMyInvitationsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedDtos);
        _mapperMock.Verify(m => m.Map<List<EventDto>>(invitationsList), Times.Once);
    }
}
using EventHub.Application.Features.Admin.Queries.GetOrganizerRequests;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Admin.Queries.GetOrganizerRequests;

public class GetOrganizerRequestsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<User>> _userRepositoryMock;
    private readonly GetOrganizerRequestsQueryHandler _handler;

    public GetOrganizerRequestsQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepositoryMock = new Mock<IGenericRepository<User>>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);

        _handler = new GetOrganizerRequestsQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUsersHaveRequestedOrganizerRole_ShouldReturnFilteredRequests()
    {
        var users = new List<User>
        {
            new User { FullName = "Pera Perić", Email = "pera@mail.com", City = "Beograd", IsOrganizerRequested = true, OrganizerRequestStatus = "Pending" },
            new User { FullName = "Mika Mikić", Email = "mika@mail.com", City = "Novi Sad", IsOrganizerRequested = false, OrganizerRequestStatus = "None" },
            new User { FullName = "Ana Anić", Email = "ana@mail.com", City = "Niš", IsOrganizerRequested = true, OrganizerRequestStatus = "Pending" }
        };

        _userRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(users);

        var query = new GetOrganizerRequestsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.FullName == "Pera Perić" && r.Email == "pera@mail.com");
        result.Should().Contain(r => r.FullName == "Ana Anić" && r.Email == "ana@mail.com");
        result.Should().NotContain(r => r.FullName == "Mika Mikić");

        _userRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoUsersRequestedOrganizerRole_ShouldReturnEmptyList()
    {
        var users = new List<User>
        {
            new User { FullName = "Mika Mikić", Email = "mika@mail.com", City = "Novi Sad", IsOrganizerRequested = false, OrganizerRequestStatus = "None" }
        };

        _userRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(users);

        var query = new GetOrganizerRequestsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _userRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }
}
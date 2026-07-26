using EventHub.Application.Features.Admin.Queries.GetUsers;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Admin.Queries.GetUsers;

public class GetUsersQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<User>> _userRepositoryMock;
    private readonly GetUsersQueryHandler _handler;

    public GetUsersQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepositoryMock = new Mock<IGenericRepository<User>>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);

        _handler = new GetUsersQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUsersExist_ShouldReturnUsersExceptAdmins()
    {
        var users = new List<User>
        {
            new User { FullName = "Pera Perić", Role = UserRole.Attendee },
            new User { FullName = "Mika Mikić", Role = UserRole.Organizer },
            new User { FullName = "Admin Adminović", Role = UserRole.Admin }
        };

        _userRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(users);

        var query = new GetUsersQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(u => u.FullName == "Pera Perić");
        result.Should().Contain(u => u.FullName == "Mika Mikić");
        result.Should().NotContain(u => u.FullName == "Admin Adminović");

        _userRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenOnlyAdminsExist_ShouldReturnEmptyList()
    {
        var users = new List<User>
        {
            new User { FullName = "Admin Adminović", Role = UserRole.Admin }
        };

        _userRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(users);

        var query = new GetUsersQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _userRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }
}
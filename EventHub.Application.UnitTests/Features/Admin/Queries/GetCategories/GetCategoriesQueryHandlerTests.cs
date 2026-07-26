using EventHub.Application.Features.Admin.Queries.GetCategories;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Admin.Queries.GetCategories;

public class GetCategoriesQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<Category>> _categoryRepositoryMock;
    private readonly GetCategoriesQueryHandler _handler;

    public GetCategoriesQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _categoryRepositoryMock = new Mock<IGenericRepository<Category>>();

        _unitOfWorkMock.Setup(u => u.Categories).Returns(_categoryRepositoryMock.Object);

        _handler = new GetCategoriesQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCategoriesExist_ShouldReturnListOfCategories()
    {
        var categories = new List<Category>
        {
            new Category { Name = "Muzika", Description = "Muzički događaji" },
            new Category { Name = "Sport", Description = "Sportski događaji" }
        };

        _categoryRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(categories);

        var query = new GetCategoriesQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(categories);

        _categoryRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoCategoriesExist_ShouldReturnEmptyList()
    {
        _categoryRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Category>());

        var query = new GetCategoriesQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _categoryRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }
}
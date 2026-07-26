using EventHub.Application.Features.Categories.Queries.GetCategories;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Categories.Queries.GetCategories;

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
    public async Task Handle_WhenCategoriesExist_ShouldReturnListOfCategoryDtos()
    {
        var categoryId1 = Guid.NewGuid();
        var categoryId2 = Guid.NewGuid();

        var category1 = new Category();
        typeof(Category).GetProperty(nameof(Category.Id))?.SetValue(category1, categoryId1);
        typeof(Category).GetProperty(nameof(Category.Name))?.SetValue(category1, "Muzika");

        var category2 = new Category();
        typeof(Category).GetProperty(nameof(Category.Id))?.SetValue(category2, categoryId2);
        typeof(Category).GetProperty(nameof(Category.Name))?.SetValue(category2, "Sport");

        var categoriesList = new List<Category> { category1, category2 };

        _categoryRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(categoriesList);

        var query = new GetCategoriesQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        
        result[0].Id.Should().Be(categoryId1);
        result[0].Name.Should().Be("Muzika");

        result[1].Id.Should().Be(categoryId2);
        result[1].Name.Should().Be("Sport");
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
    }
}
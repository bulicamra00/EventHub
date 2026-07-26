using EventHub.Application.Features.Admin.Commands.CreateCategory;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace EventHub.Application.UnitTests.Features.Admin.Commands.CreateCategory;

public class CreateCategoryCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<Category>> _categoryRepositoryMock;
    private readonly CreateCategoryCommandHandler _handler;

    public CreateCategoryCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _categoryRepositoryMock = new Mock<IGenericRepository<Category>>();

        _unitOfWorkMock.Setup(u => u.Categories).Returns(_categoryRepositoryMock.Object);

        _handler = new CreateCategoryCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldAddCategoryAndReturnId()
    {
        var command = new CreateCategoryCommand("Muzika", "Opis muzičkih događaja");

        Category? capturedCategory = null;
        _categoryRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Category>()))
            .Callback<Category>(c => capturedCategory = c)
            .Returns(Task.CompletedTask);

        var resultId = await _handler.Handle(command, CancellationToken.None);

        resultId.Should().NotBeEmpty();

        capturedCategory.Should().NotBeNull();
        capturedCategory!.Name.Should().Be(command.Name);
        capturedCategory.Description.Should().Be(command.Description);
        capturedCategory.Id.Should().Be(resultId);

        _categoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }
}
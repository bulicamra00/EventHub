namespace EventHub.Application.Features.Categories.Queries.GetCategories;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
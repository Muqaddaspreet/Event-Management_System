using EventManagement.API.DTOs.Categories;
using EventManagement.API.Entities;
using EventManagement.API.Repositories.Interfaces;
using EventManagement.API.Services;
using Moq;

namespace EventManagement.Tests.Services;

public class CategoryServiceTests
{
    private static CategoryService BuildService(Mock<ICategoryRepository> repo) =>
        new(repo.Object);

    [Fact]
    public async Task Create_WithDuplicateName_ReturnsConflict()
    {
        var repo = new Mock<ICategoryRepository>();
        repo.Setup(r => r.NameExistsAsync("Tech")).ReturnsAsync(true);
        var svc = BuildService(repo);

        var result = await svc.CreateAsync(new CreateCategoryRequest { Name = "Tech" });

        Assert.False(result.Succeeded);
        Assert.True(result.IsConflict);
        repo.Verify(r => r.CreateAsync(It.IsAny<Category>()), Times.Never);
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsResponse()
    {
        Category? captured = null;
        var repo = new Mock<ICategoryRepository>();
        repo.Setup(r => r.NameExistsAsync("Music")).ReturnsAsync(false);
        repo.Setup(r => r.CreateAsync(It.IsAny<Category>()))
            .Callback<Category>(c => { captured = c; c.Id = 1; })
            .ReturnsAsync((Category c) => c);
        var svc = BuildService(repo);

        var result = await svc.CreateAsync(new CreateCategoryRequest { Name = "Music" });

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("Music", result.Data!.Name);
        Assert.Equal(1, result.Data.Id);
    }

    [Fact]
    public async Task Delete_WithReferencedEvents_ReturnsConflict()
    {
        var repo = new Mock<ICategoryRepository>();
        repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Category { Id = 1, Name = "Tech" });
        repo.Setup(r => r.HasEventsAsync(1)).ReturnsAsync(true);
        var svc = BuildService(repo);

        var result = await svc.DeleteAsync(1);

        Assert.False(result.Succeeded);
        Assert.True(result.IsConflict);
        repo.Verify(r => r.DeleteAsync(It.IsAny<Category>()), Times.Never);
    }

    [Fact]
    public async Task Delete_NotFound_ReturnsNotFound()
    {
        var repo = new Mock<ICategoryRepository>();
        repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Category?)null);
        var svc = BuildService(repo);

        var result = await svc.DeleteAsync(99);

        Assert.False(result.Succeeded);
        Assert.True(result.IsNotFound);
    }

    [Fact]
    public async Task Update_WithDuplicateNameOnOther_ReturnsConflict()
    {
        var repo = new Mock<ICategoryRepository>();
        repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Category { Id = 1, Name = "Tech" });
        repo.Setup(r => r.NameExistsForOtherAsync("Music", 1)).ReturnsAsync(true);
        var svc = BuildService(repo);

        var result = await svc.UpdateAsync(1, new UpdateCategoryRequest { Name = "Music" });

        Assert.False(result.Succeeded);
        Assert.True(result.IsConflict);
    }
}
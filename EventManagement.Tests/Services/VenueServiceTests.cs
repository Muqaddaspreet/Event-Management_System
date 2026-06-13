using EventManagement.API.DTOs.Venues;
using EventManagement.API.Entities;
using EventManagement.API.Repositories.Interfaces;
using EventManagement.API.Services;
using Moq;

namespace EventManagement.Tests.Services;

public class VenueServiceTests
{
    private static VenueService BuildService(Mock<IVenueRepository> repo) =>
        new(repo.Object);

    [Fact]
    public async Task Create_WithZeroCapacity_ReturnsFailure()
    {
        var repo = new Mock<IVenueRepository>();
        var svc = BuildService(repo);

        var result = await svc.CreateAsync(new CreateVenueRequest
        {
            Name     = "City Hall",
            Address  = "1 Main St",
            City     = "Lahore",
            Capacity = 0
        });

        Assert.False(result.Succeeded);
        Assert.False(result.IsConflict);
        repo.Verify(r => r.CreateAsync(It.IsAny<Venue>()), Times.Never);
    }

    [Fact]
    public async Task Create_WithNegativeCapacity_ReturnsFailure()
    {
        var repo = new Mock<IVenueRepository>();
        var svc = BuildService(repo);

        var result = await svc.CreateAsync(new CreateVenueRequest
        {
            Name     = "City Hall",
            Address  = "1 Main St",
            City     = "Lahore",
            Capacity = -5
        });

        Assert.False(result.Succeeded);
        repo.Verify(r => r.CreateAsync(It.IsAny<Venue>()), Times.Never);
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsResponse()
    {
        Venue? captured = null;
        var repo = new Mock<IVenueRepository>();
        repo.Setup(r => r.CreateAsync(It.IsAny<Venue>()))
            .Callback<Venue>(v => { captured = v; v.Id = 1; })
            .ReturnsAsync((Venue v) => v);
        var svc = BuildService(repo);

        var result = await svc.CreateAsync(new CreateVenueRequest
        {
            Name     = "Convention Centre",
            Address  = "5 Event Ave",
            City     = "Karachi",
            Capacity = 500
        });

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("Convention Centre", result.Data!.Name);
        Assert.Equal("Karachi", result.Data.City);
        Assert.Equal(500, result.Data.Capacity);
        Assert.Equal(1, result.Data.Id);
    }

    [Fact]
    public async Task Delete_WithReferencedEvents_ReturnsConflict()
    {
        var repo = new Mock<IVenueRepository>();
        repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Venue { Id = 1, Name = "Hall A" });
        repo.Setup(r => r.HasEventsAsync(1)).ReturnsAsync(true);
        var svc = BuildService(repo);

        var result = await svc.DeleteAsync(1);

        Assert.False(result.Succeeded);
        Assert.True(result.IsConflict);
        repo.Verify(r => r.DeleteAsync(It.IsAny<Venue>()), Times.Never);
    }

    [Fact]
    public async Task Delete_NotFound_ReturnsNotFound()
    {
        var repo = new Mock<IVenueRepository>();
        repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Venue?)null);
        var svc = BuildService(repo);

        var result = await svc.DeleteAsync(99);

        Assert.False(result.Succeeded);
        Assert.True(result.IsNotFound);
    }
}
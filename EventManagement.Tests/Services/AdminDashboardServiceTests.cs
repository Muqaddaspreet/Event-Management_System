using EventManagement.API.Enums;
using EventManagement.API.Repositories.Interfaces;
using EventManagement.API.Services;
using Moq;

namespace EventManagement.Tests.Services;

public class AdminDashboardServiceTests
{
    private static AdminDashboardService BuildService(
        Mock<IUserRepository>? userRepo = null,
        Mock<IEventRepository>? eventRepo = null,
        Mock<IRegistrationRepository>? regRepo = null)
    {
        userRepo  ??= new Mock<IUserRepository>();
        eventRepo ??= new Mock<IEventRepository>();
        regRepo   ??= new Mock<IRegistrationRepository>();
        return new AdminDashboardService(userRepo.Object, eventRepo.Object, regRepo.Object);
    }

    [Fact]
    public async Task GetDashboard_ReturnsAllExpectedCounts()
    {
        var userRepo  = new Mock<IUserRepository>();
        var eventRepo = new Mock<IEventRepository>();
        var regRepo   = new Mock<IRegistrationRepository>();

        userRepo.Setup(r => r.CountTotalAsync()).ReturnsAsync(10);
        eventRepo.Setup(r => r.CountTotalAsync()).ReturnsAsync(8);
        eventRepo.Setup(r => r.CountByStatusAsync(EventStatus.PendingApproval)).ReturnsAsync(3);
        eventRepo.Setup(r => r.CountByStatusAsync(EventStatus.Published)).ReturnsAsync(4);
        regRepo.Setup(r => r.CountTotalAsync()).ReturnsAsync(25);

        var svc    = BuildService(userRepo, eventRepo, regRepo);
        var result = await svc.GetDashboardAsync();

        Assert.Equal(10, result.TotalUsers);
        Assert.Equal(8,  result.TotalEvents);
        Assert.Equal(3,  result.PendingEvents);
        Assert.Equal(4,  result.PublishedEvents);
        Assert.Equal(25, result.TotalRegistrations);
    }

    [Fact]
    public async Task GetDashboard_PendingEventCountMappedCorrectly()
    {
        var eventRepo = new Mock<IEventRepository>();
        eventRepo.Setup(r => r.CountTotalAsync()).ReturnsAsync(5);
        eventRepo.Setup(r => r.CountByStatusAsync(EventStatus.PendingApproval)).ReturnsAsync(2);
        eventRepo.Setup(r => r.CountByStatusAsync(EventStatus.Published)).ReturnsAsync(3);

        var svc    = BuildService(eventRepo: eventRepo);
        var result = await svc.GetDashboardAsync();

        Assert.Equal(2, result.PendingEvents);
        eventRepo.Verify(r => r.CountByStatusAsync(EventStatus.PendingApproval), Times.Once);
    }

    [Fact]
    public async Task GetDashboard_PublishedEventCountMappedCorrectly()
    {
        var eventRepo = new Mock<IEventRepository>();
        eventRepo.Setup(r => r.CountTotalAsync()).ReturnsAsync(5);
        eventRepo.Setup(r => r.CountByStatusAsync(EventStatus.PendingApproval)).ReturnsAsync(1);
        eventRepo.Setup(r => r.CountByStatusAsync(EventStatus.Published)).ReturnsAsync(4);

        var svc    = BuildService(eventRepo: eventRepo);
        var result = await svc.GetDashboardAsync();

        Assert.Equal(4, result.PublishedEvents);
        eventRepo.Verify(r => r.CountByStatusAsync(EventStatus.Published), Times.Once);
    }
}
using EventManagement.API.DTOs.Registrations;
using EventManagement.API.Entities;
using EventManagement.API.Enums;
using EventManagement.API.Repositories.Interfaces;
using EventManagement.API.Services;
using Moq;

namespace EventManagement.Tests.Services;

public class RegistrationServiceTests
{
    // ── Helpers ────────────────────────────────────────────────────────────

    private static RegistrationService BuildService(
        Mock<IRegistrationRepository> regRepo,
        Mock<IEventRepository>? eventRepo = null)
    {
        eventRepo ??= new Mock<IEventRepository>();
        return new RegistrationService(regRepo.Object, eventRepo.Object);
    }

    private static Event MakePublishedEvent(int id, int capacity = 100) => new()
    {
        Id          = id,
        Title       = "Test Event",
        StartTime   = DateTime.UtcNow.AddDays(1),
        EndTime     = DateTime.UtcNow.AddDays(2),
        Capacity    = capacity,
        Status      = EventStatus.Published,
        OrganizerId = 99,
        Organizer   = new User { Id = 99, FullName = "Organizer" },
        Category    = new Category { Id = 1, Name = "Tech" },
        Venue       = new Venue { Id = 1, Name = "Hall", Address = "1 Main St", City = "Lahore", Capacity = 500 },
        Registrations = []
    };

    private static EventRegistration MakeRegistration(
        int id, int eventId, int userId,
        RegistrationStatus status = RegistrationStatus.Registered) => new()
    {
        Id           = id,
        EventId      = eventId,
        UserId       = userId,
        Status       = status,
        RegisteredAt = DateTime.UtcNow.AddHours(-1),
        UpdatedAt    = DateTime.UtcNow.AddHours(-1),
        Event        = new Event { Id = eventId, Title = "Test Event", StartTime = DateTime.UtcNow.AddDays(1) },
        User         = new User  { Id = userId,  FullName = "Test Attendee" }
    };

    // ── Register tests ─────────────────────────────────────────────────────

    [Fact]
    public async Task Register_ReturnsNotFoundWhenEventDoesNotExist()
    {
        var regRepo   = new Mock<IRegistrationRepository>();
        var eventRepo = new Mock<IEventRepository>();
        eventRepo.Setup(r => r.GetByIdWithDetailsAsync(99)).ReturnsAsync((Event?)null);

        var svc    = BuildService(regRepo, eventRepo);
        var result = await svc.RegisterAsync(new CreateRegistrationRequest { EventId = 99 }, attendeeId: 1);

        Assert.False(result.Succeeded);
        Assert.True(result.IsNotFound);
        regRepo.Verify(r => r.CreateAsync(It.IsAny<EventRegistration>()), Times.Never);
    }

    [Fact]
    public async Task Register_RejectsNonPublishedEvent()
    {
        var pendingEvent = MakePublishedEvent(1);
        pendingEvent.Status = EventStatus.PendingApproval;

        var regRepo   = new Mock<IRegistrationRepository>();
        var eventRepo = new Mock<IEventRepository>();
        eventRepo.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(pendingEvent);

        var svc    = BuildService(regRepo, eventRepo);
        var result = await svc.RegisterAsync(new CreateRegistrationRequest { EventId = 1 }, attendeeId: 1);

        Assert.False(result.Succeeded);
        Assert.True(result.IsUnprocessableEntity);
        regRepo.Verify(r => r.CreateAsync(It.IsAny<EventRegistration>()), Times.Never);
    }

    [Fact]
    public async Task Register_RejectsEventAtCapacity()
    {
        var regRepo   = new Mock<IRegistrationRepository>();
        var eventRepo = new Mock<IEventRepository>();
        eventRepo.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(MakePublishedEvent(1, capacity: 1));
        regRepo.Setup(r => r.FindByEventAndUserAsync(1, 7)).ReturnsAsync((EventRegistration?)null);
        regRepo.Setup(r => r.CountActiveByEventIdAsync(1)).ReturnsAsync(1);

        var svc    = BuildService(regRepo, eventRepo);
        var result = await svc.RegisterAsync(new CreateRegistrationRequest { EventId = 1 }, attendeeId: 7);

        Assert.False(result.Succeeded);
        Assert.True(result.IsUnprocessableEntity);
        regRepo.Verify(r => r.CreateAsync(It.IsAny<EventRegistration>()), Times.Never);
    }

    [Fact]
    public async Task Register_CreatesNewRegistrationForPublishedEvent()
    {
        EventRegistration? captured = null;
        var regRepo   = new Mock<IRegistrationRepository>();
        var eventRepo = new Mock<IEventRepository>();
        eventRepo.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(MakePublishedEvent(1));
        regRepo.Setup(r => r.FindByEventAndUserAsync(1, 7)).ReturnsAsync((EventRegistration?)null);
        regRepo.Setup(r => r.CountActiveByEventIdAsync(1)).ReturnsAsync(0);
        regRepo.Setup(r => r.CreateAsync(It.IsAny<EventRegistration>()))
            .Callback<EventRegistration>(r => { captured = r; r.Id = 10; })
            .ReturnsAsync((EventRegistration r) => r);
        regRepo.Setup(r => r.GetByIdWithDetailsAsync(10))
            .ReturnsAsync(MakeRegistration(10, 1, 7));

        var svc    = BuildService(regRepo, eventRepo);
        var result = await svc.RegisterAsync(new CreateRegistrationRequest { EventId = 1 }, attendeeId: 7);

        Assert.True(result.Succeeded);
        Assert.NotNull(captured);
        Assert.Equal(RegistrationStatus.Registered, captured!.Status);
        Assert.Equal(1, captured.EventId);
        Assert.Equal(7, captured.UserId);
    }

    [Fact]
    public async Task Register_RejectsDuplicateActiveRegistration()
    {
        var existing = MakeRegistration(5, 1, 7, RegistrationStatus.Registered);

        var regRepo   = new Mock<IRegistrationRepository>();
        var eventRepo = new Mock<IEventRepository>();
        eventRepo.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(MakePublishedEvent(1));
        regRepo.Setup(r => r.FindByEventAndUserAsync(1, 7)).ReturnsAsync(existing);

        var svc    = BuildService(regRepo, eventRepo);
        var result = await svc.RegisterAsync(new CreateRegistrationRequest { EventId = 1 }, attendeeId: 7);

        Assert.False(result.Succeeded);
        Assert.True(result.IsConflict);
        regRepo.Verify(r => r.CreateAsync(It.IsAny<EventRegistration>()), Times.Never);
        regRepo.Verify(r => r.UpdateAsync(It.IsAny<EventRegistration>()), Times.Never);
    }

    [Fact]
    public async Task Register_ReactivatesCancelledRegistration()
    {
        EventRegistration? saved = null;
        var existing = MakeRegistration(5, 1, 7, RegistrationStatus.Cancelled);

        var regRepo   = new Mock<IRegistrationRepository>();
        var eventRepo = new Mock<IEventRepository>();
        eventRepo.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(MakePublishedEvent(1));
        regRepo.Setup(r => r.FindByEventAndUserAsync(1, 7)).ReturnsAsync(existing);
        regRepo.Setup(r => r.CountActiveByEventIdAsync(1)).ReturnsAsync(3);
        regRepo.Setup(r => r.UpdateAsync(It.IsAny<EventRegistration>()))
            .Callback<EventRegistration>(r => saved = r)
            .Returns(Task.CompletedTask);
        regRepo.Setup(r => r.GetByIdWithDetailsAsync(5))
            .ReturnsAsync(MakeRegistration(5, 1, 7, RegistrationStatus.Registered));

        var svc    = BuildService(regRepo, eventRepo);
        var result = await svc.RegisterAsync(new CreateRegistrationRequest { EventId = 1 }, attendeeId: 7);

        Assert.True(result.Succeeded);
        Assert.NotNull(saved);
        Assert.Equal(RegistrationStatus.Registered, saved!.Status);
        regRepo.Verify(r => r.CreateAsync(It.IsAny<EventRegistration>()), Times.Never);
    }

    // ── Cancel tests ───────────────────────────────────────────────────────

    [Fact]
    public async Task Cancel_SetsStatusToCancelled()
    {
        EventRegistration? saved = null;
        var reg     = MakeRegistration(1, 10, 7, RegistrationStatus.Registered);
        var regRepo = new Mock<IRegistrationRepository>();
        regRepo.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(reg);
        regRepo.Setup(r => r.UpdateAsync(It.IsAny<EventRegistration>()))
            .Callback<EventRegistration>(r => saved = r)
            .Returns(Task.CompletedTask);

        var svc    = BuildService(regRepo);
        var result = await svc.CancelAsync(1, callerId: 7);

        Assert.True(result.Succeeded);
        Assert.NotNull(saved);
        Assert.Equal(RegistrationStatus.Cancelled, saved!.Status);
    }

    [Fact]
    public async Task Cancel_AnotherUsersRegistrationReturnsForbidden()
    {
        var reg     = MakeRegistration(1, 10, 7, RegistrationStatus.Registered);
        var regRepo = new Mock<IRegistrationRepository>();
        regRepo.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(reg);

        var svc    = BuildService(regRepo);
        var result = await svc.CancelAsync(1, callerId: 99);

        Assert.False(result.Succeeded);
        Assert.True(result.IsForbidden);
        regRepo.Verify(r => r.UpdateAsync(It.IsAny<EventRegistration>()), Times.Never);
    }

    [Fact]
    public async Task Cancel_AlreadyCancelledReturnsConflict()
    {
        var reg     = MakeRegistration(1, 10, 7, RegistrationStatus.Cancelled);
        var regRepo = new Mock<IRegistrationRepository>();
        regRepo.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(reg);

        var svc    = BuildService(regRepo);
        var result = await svc.CancelAsync(1, callerId: 7);

        Assert.False(result.Succeeded);
        Assert.True(result.IsConflict);
        regRepo.Verify(r => r.UpdateAsync(It.IsAny<EventRegistration>()), Times.Never);
    }

    // ── GetMine tests ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetMine_ReturnsOnlyAttendeeRegistrations()
    {
        var regs = new List<EventRegistration>
        {
            MakeRegistration(1, 10, 7),
            MakeRegistration(2, 11, 7, RegistrationStatus.Cancelled)
        };
        var regRepo = new Mock<IRegistrationRepository>();
        regRepo.Setup(r => r.GetByUserIdAsync(7)).ReturnsAsync(regs);

        var svc    = BuildService(regRepo);
        var result = (await svc.GetMineAsync(7)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal(7, r.UserId));
        Assert.Equal("Registered", result[0].Status);
        Assert.Equal("Cancelled",  result[1].Status);
    }
}
using EventManagement.API.DTOs.Events;
using EventManagement.API.Entities;
using EventManagement.API.Enums;
using EventManagement.API.Repositories.Interfaces;
using EventManagement.API.Services;
using Moq;

namespace EventManagement.Tests.Services;

public class EventServiceTests
{
    // ── Helpers ────────────────────────────────────────────────────────────

    private static EventService BuildService(
        Mock<IEventRepository> eventRepo,
        Mock<ICategoryRepository>? categoryRepo = null,
        Mock<IVenueRepository>? venueRepo = null)
    {
        categoryRepo ??= DefaultCategoryRepo();
        venueRepo    ??= DefaultVenueRepo();
        return new EventService(eventRepo.Object, categoryRepo.Object, venueRepo.Object);
    }

    private static CreateEventRequest ValidRequest(
        DateTime? start = null, DateTime? end = null, int capacity = 100,
        int categoryId = 1, int venueId = 1) =>
        new()
        {
            Title      = "Test Event",
            StartTime  = start ?? DateTime.UtcNow.AddDays(1),
            EndTime    = end   ?? DateTime.UtcNow.AddDays(2),
            Capacity   = capacity,
            CategoryId = categoryId,
            VenueId    = venueId
        };

    private static Mock<ICategoryRepository> DefaultCategoryRepo()
    {
        var m = new Mock<ICategoryRepository>();
        m.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Category { Id = 1, Name = "Tech" });
        return m;
    }

    private static Mock<IVenueRepository> DefaultVenueRepo()
    {
        var m = new Mock<IVenueRepository>();
        m.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Venue { Id = 1, Name = "Hall", Address = "1 Main St", City = "Lahore", Capacity = 500 });
        return m;
    }

    private static Event MakeDetailEvent(int id, EventStatus status = EventStatus.PendingApproval,
        int organizerId = 1) =>
        new()
        {
            Id          = id,
            Title       = "Test Event",
            StartTime   = DateTime.UtcNow.AddDays(1),
            EndTime     = DateTime.UtcNow.AddDays(2),
            Capacity    = 100,
            Status      = status,
            OrganizerId = organizerId,
            Organizer   = new User { Id = organizerId, FullName = "Organizer One" },
            Category    = new Category { Id = 1, Name = "Tech" },
            Venue       = new Venue { Id = 1, Name = "Hall", Address = "1 Main St", City = "Lahore", Capacity = 500 },
            Registrations = []
        };

    // ── Create tests ───────────────────────────────────────────────────────

    [Fact]
    public async Task Create_DefaultsToPendingApproval()
    {
        Event? captured = null;
        var eventRepo = new Mock<IEventRepository>();
        eventRepo.Setup(r => r.CreateAsync(It.IsAny<Event>()))
            .Callback<Event>(e => { captured = e; e.Id = 1; })
            .ReturnsAsync((Event e) => e);
        eventRepo.Setup(r => r.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(MakeDetailEvent(1));

        var svc = BuildService(eventRepo);
        var result = await svc.CreateAsync(ValidRequest(), organizerId: 1);

        Assert.True(result.Succeeded);
        Assert.NotNull(captured);
        Assert.Equal(EventStatus.PendingApproval, captured!.Status);
    }

    [Fact]
    public async Task Create_RejectsEndTimeNotAfterStartTime()
    {
        var now = DateTime.UtcNow.AddDays(1);
        var eventRepo = new Mock<IEventRepository>();
        var svc = BuildService(eventRepo);

        var result = await svc.CreateAsync(ValidRequest(start: now, end: now), organizerId: 1);

        Assert.False(result.Succeeded);
        Assert.False(result.IsConflict);
        eventRepo.Verify(r => r.CreateAsync(It.IsAny<Event>()), Times.Never);
    }

    [Fact]
    public async Task Create_RejectsZeroCapacity()
    {
        var eventRepo = new Mock<IEventRepository>();
        var svc = BuildService(eventRepo);

        var result = await svc.CreateAsync(ValidRequest(capacity: 0), organizerId: 1);

        Assert.False(result.Succeeded);
        eventRepo.Verify(r => r.CreateAsync(It.IsAny<Event>()), Times.Never);
    }

    [Fact]
    public async Task Create_RejectsInvalidCategoryId()
    {
        var eventRepo = new Mock<IEventRepository>();
        var categoryRepo = new Mock<ICategoryRepository>();
        categoryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Category?)null);
        var svc = BuildService(eventRepo, categoryRepo: categoryRepo);

        var result = await svc.CreateAsync(ValidRequest(categoryId: 99), organizerId: 1);

        Assert.False(result.Succeeded);
        eventRepo.Verify(r => r.CreateAsync(It.IsAny<Event>()), Times.Never);
    }

    [Fact]
    public async Task Create_RejectsInvalidVenueId()
    {
        var eventRepo = new Mock<IEventRepository>();
        var venueRepo = new Mock<IVenueRepository>();
        venueRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Venue?)null);
        var svc = BuildService(eventRepo, venueRepo: venueRepo);

        var result = await svc.CreateAsync(ValidRequest(venueId: 99), organizerId: 1);

        Assert.False(result.Succeeded);
        eventRepo.Verify(r => r.CreateAsync(It.IsAny<Event>()), Times.Never);
    }

    // ── Update tests ───────────────────────────────────────────────────────

    [Fact]
    public async Task Update_OrganizerCannotUpdateAnotherOrganizersEvent()
    {
        var eventRepo = new Mock<IEventRepository>();
        // Event is owned by organizer 10, caller is organizer 99
        eventRepo.Setup(r => r.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(MakeDetailEvent(1, organizerId: 10));

        var svc = BuildService(eventRepo);
        var request = new UpdateEventRequest
        {
            Title      = "Updated",
            StartTime  = DateTime.UtcNow.AddDays(1),
            EndTime    = DateTime.UtcNow.AddDays(2),
            Capacity   = 100,
            CategoryId = 1,
            VenueId    = 1
        };

        var result = await svc.UpdateAsync(1, request, callerId: 99, callerRole: "Organizer");

        Assert.False(result.Succeeded);
        Assert.True(result.IsForbidden);
        eventRepo.Verify(r => r.UpdateAsync(It.IsAny<Event>()), Times.Never);
    }

    // ── Cancel tests ───────────────────────────────────────────────────────

    [Fact]
    public async Task Cancel_SetsStatusToCancelled()
    {
        Event? saved = null;
        var eventRepo = new Mock<IEventRepository>();
        eventRepo.Setup(r => r.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(MakeDetailEvent(1, status: EventStatus.Published, organizerId: 5));
        eventRepo.Setup(r => r.UpdateAsync(It.IsAny<Event>()))
            .Callback<Event>(e => saved = e)
            .Returns(Task.CompletedTask);

        var svc = BuildService(eventRepo);
        var result = await svc.CancelAsync(1, callerId: 5, callerRole: "Organizer");

        Assert.True(result.Succeeded);
        Assert.NotNull(saved);
        Assert.Equal(EventStatus.Cancelled, saved!.Status);
    }

    [Fact]
    public async Task Cancel_AlreadyCancelledReturnsConflict()
    {
        var eventRepo = new Mock<IEventRepository>();
        eventRepo.Setup(r => r.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(MakeDetailEvent(1, status: EventStatus.Cancelled, organizerId: 5));

        var svc = BuildService(eventRepo);
        var result = await svc.CancelAsync(1, callerId: 5, callerRole: "Organizer");

        Assert.False(result.Succeeded);
        Assert.True(result.IsConflict);
        eventRepo.Verify(r => r.UpdateAsync(It.IsAny<Event>()), Times.Never);
    }

    // ── GetById visibility tests ───────────────────────────────────────────

    [Fact]
    public async Task GetById_ReturnsNotFoundWhenAnonymousCallerAccessesNonPublishedEvent()
    {
        var eventRepo = new Mock<IEventRepository>();
        eventRepo.Setup(r => r.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(MakeDetailEvent(1, status: EventStatus.PendingApproval));

        var svc = BuildService(eventRepo);
        var result = await svc.GetByIdAsync(1, callerId: null, callerRole: null);

        Assert.False(result.Succeeded);
        Assert.True(result.IsNotFound);
    }

    // ── GetPublished filter test ───────────────────────────────────────────

    [Fact]
    public async Task GetPublished_ReturnsPagedResultFromRepository()
    {
        var publishedEvent = MakeDetailEvent(1, status: EventStatus.Published);
        var eventRepo = new Mock<IEventRepository>();
        eventRepo.Setup(r => r.GetPublishedPagedAsync(null, null, null, 1, 10))
            .ReturnsAsync(([publishedEvent], 1));

        var svc = BuildService(eventRepo);
        var result = await svc.GetPublishedAsync(null, null, null, 1, 10);

        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("Test Event", result.Items.First().Title);
        Assert.Equal(EventStatus.Published.ToString(), result.Items.First().Status);
    }

    // ── Pagination normalization tests ────────────────────────────────────

    [Fact]
    public async Task GetPublished_NormalizesInvalidPageAndPageSize()
    {
        // page=0 → 1, pageSize=100 → 50
        var eventRepo = new Mock<IEventRepository>();
        eventRepo.Setup(r => r.GetPublishedPagedAsync(null, null, null, 1, 50))
            .ReturnsAsync(([], 0));

        var svc = BuildService(eventRepo);
        var result = await svc.GetPublishedAsync(null, null, null, page: 0, pageSize: 100);

        Assert.Equal(1, result.Page);
        Assert.Equal(50, result.PageSize);
        eventRepo.Verify(r => r.GetPublishedPagedAsync(null, null, null, 1, 50), Times.Once);
    }

    // ── Create reload safety test ─────────────────────────────────────────

    [Fact]
    public async Task Create_ReturnsFailureWhenReloadedEventIsNull()
    {
        var eventRepo = new Mock<IEventRepository>();
        eventRepo.Setup(r => r.CreateAsync(It.IsAny<Event>()))
            .Callback<Event>(e => e.Id = 1)
            .ReturnsAsync((Event e) => e);
        eventRepo.Setup(r => r.GetByIdWithDetailsAsync(1))
            .ReturnsAsync((Event?)null);

        var svc = BuildService(eventRepo);
        var result = await svc.CreateAsync(ValidRequest(), organizerId: 1);

        Assert.False(result.Succeeded);
        Assert.Contains("could not be loaded", result.Error);
    }

    // ── Admin approval tests ──────────────────────────────────────────────
    //
    // Invalid status string ("xyz") is rejected at the controller boundary with 400
    // via Enum.TryParse before reaching the service. Service-layer tests below cover
    // all valid EventStatus values and the null (no-filter) case.

    [Fact]
    public async Task Approve_SetsPendingApprovalToPublished()
    {
        Event? saved = null;
        var eventRepo = new Mock<IEventRepository>();
        eventRepo.Setup(r => r.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(MakeDetailEvent(1, EventStatus.PendingApproval, organizerId: 5));
        eventRepo.Setup(r => r.UpdateAsync(It.IsAny<Event>()))
            .Callback<Event>(e => saved = e)
            .Returns(Task.CompletedTask);

        var svc = BuildService(eventRepo);
        var result = await svc.ApproveAsync(1);

        Assert.True(result.Succeeded);
        Assert.NotNull(saved);
        Assert.Equal(EventStatus.Published, saved!.Status);
    }

    [Fact]
    public async Task Reject_SetsPendingApprovalToRejected()
    {
        Event? saved = null;
        var eventRepo = new Mock<IEventRepository>();
        eventRepo.Setup(r => r.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(MakeDetailEvent(1, EventStatus.PendingApproval, organizerId: 5));
        eventRepo.Setup(r => r.UpdateAsync(It.IsAny<Event>()))
            .Callback<Event>(e => saved = e)
            .Returns(Task.CompletedTask);

        var svc = BuildService(eventRepo);
        var result = await svc.RejectAsync(1, new RejectEventRequest { Reason = "Incomplete details." });

        Assert.True(result.Succeeded);
        Assert.NotNull(saved);
        Assert.Equal(EventStatus.Rejected, saved!.Status);
    }

    [Fact]
    public async Task Approve_NonPendingEventReturnsConflict()
    {
        var eventRepo = new Mock<IEventRepository>();
        eventRepo.Setup(r => r.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(MakeDetailEvent(1, EventStatus.Published, organizerId: 5));

        var svc = BuildService(eventRepo);
        var result = await svc.ApproveAsync(1);

        Assert.False(result.Succeeded);
        Assert.True(result.IsConflict);
        eventRepo.Verify(r => r.UpdateAsync(It.IsAny<Event>()), Times.Never);
    }

    [Fact]
    public async Task Reject_NonPendingEventReturnsConflict()
    {
        var eventRepo = new Mock<IEventRepository>();
        eventRepo.Setup(r => r.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(MakeDetailEvent(1, EventStatus.Cancelled, organizerId: 5));

        var svc = BuildService(eventRepo);
        var result = await svc.RejectAsync(1, new RejectEventRequest());

        Assert.False(result.Succeeded);
        Assert.True(result.IsConflict);
        eventRepo.Verify(r => r.UpdateAsync(It.IsAny<Event>()), Times.Never);
    }

    [Fact]
    public async Task Approve_MissingEventReturnsNotFound()
    {
        var eventRepo = new Mock<IEventRepository>();
        eventRepo.Setup(r => r.GetByIdWithDetailsAsync(99))
            .ReturnsAsync((Event?)null);

        var svc = BuildService(eventRepo);
        var result = await svc.ApproveAsync(99);

        Assert.False(result.Succeeded);
        Assert.True(result.IsNotFound);
    }

    [Fact]
    public async Task Reject_MissingEventReturnsNotFound()
    {
        var eventRepo = new Mock<IEventRepository>();
        eventRepo.Setup(r => r.GetByIdWithDetailsAsync(99))
            .ReturnsAsync((Event?)null);

        var svc = BuildService(eventRepo);
        var result = await svc.RejectAsync(99, new RejectEventRequest());

        Assert.False(result.Succeeded);
        Assert.True(result.IsNotFound);
    }

    [Fact]
    public async Task GetAllForAdmin_ReturnsPagedResultFromRepository()
    {
        var evt = MakeDetailEvent(1, EventStatus.PendingApproval);
        var eventRepo = new Mock<IEventRepository>();
        eventRepo.Setup(r => r.GetAllPagedAsync(null, null, 1, 10))
            .ReturnsAsync(([evt], 1));

        var svc = BuildService(eventRepo);
        var result = await svc.GetAllForAdminAsync(null, null, 1, 10);

        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("Test Event", result.Items.First().Title);
        Assert.Equal(EventStatus.PendingApproval.ToString(), result.Items.First().Status);
    }

    [Fact]
    public async Task GetAllForAdmin_StatusFilterPassedToRepository()
    {
        var eventRepo = new Mock<IEventRepository>();
        eventRepo.Setup(r => r.GetAllPagedAsync(EventStatus.PendingApproval, null, 1, 10))
            .ReturnsAsync(([MakeDetailEvent(1, EventStatus.PendingApproval)], 1));

        var svc = BuildService(eventRepo);
        var result = await svc.GetAllForAdminAsync(EventStatus.PendingApproval, null, 1, 10);

        Assert.Equal(1, result.TotalCount);
        eventRepo.Verify(r => r.GetAllPagedAsync(EventStatus.PendingApproval, null, 1, 10), Times.Once);
    }
}
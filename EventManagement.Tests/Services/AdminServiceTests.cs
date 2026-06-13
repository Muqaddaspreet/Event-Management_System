using EventManagement.API.Entities;
using EventManagement.API.Enums;
using EventManagement.API.Repositories.Interfaces;
using EventManagement.API.Services;
using Moq;

namespace EventManagement.Tests.Services;

public class AdminServiceTests
{
    // ── Helpers ────────────────────────────────────────────────────────────

    private static AdminService BuildService(
        Mock<IUserRepository>? userRepo = null,
        Mock<IRegistrationRepository>? regRepo = null)
    {
        userRepo ??= new Mock<IUserRepository>();
        regRepo  ??= new Mock<IRegistrationRepository>();
        return new AdminService(userRepo.Object, regRepo.Object);
    }

    private static User MakeUser(int id, string name = "Alice", string email = "alice@example.com",
        UserRole role = UserRole.Attendee) => new()
    {
        Id        = id,
        FullName  = name,
        Email     = email,
        Role      = role,
        CreatedAt = DateTime.UtcNow.AddDays(-10)
    };

    private static EventRegistration MakeRegistration(int id, int eventId = 1, int userId = 7,
        RegistrationStatus status = RegistrationStatus.Registered) => new()
    {
        Id           = id,
        EventId      = eventId,
        UserId       = userId,
        Status       = status,
        RegisteredAt = DateTime.UtcNow.AddHours(-2),
        UpdatedAt    = DateTime.UtcNow.AddHours(-1),
        Event        = new Event { Id = eventId, Title = "Test Event", StartTime = DateTime.UtcNow.AddDays(1) },
        User         = new User  { Id = userId,  FullName = "Test User", Email = "user@example.com" }
    };

    // ── GetUsersPagedAsync tests ───────────────────────────────────────────

    [Fact]
    public async Task GetUsersPaged_ReturnsPagedResultFromRepository()
    {
        var users    = new List<User> { MakeUser(1), MakeUser(2, "Bob", "bob@example.com") };
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.GetAllPagedAsync(null, null, 1, 10))
                .ReturnsAsync((users, 2));

        var svc    = BuildService(userRepo);
        var result = await svc.GetUsersPagedAsync(null, null, 1, 10);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count());
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task GetUsersPaged_RoleFilterIsPassedToRepository()
    {
        UserRole? capturedRole = null;
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.GetAllPagedAsync(It.IsAny<UserRole?>(), null, 1, 10))
                .Callback<UserRole?, string?, int, int>((role, _, _, _) => capturedRole = role)
                .ReturnsAsync((new List<User>(), 0));

        var svc = BuildService(userRepo);
        await svc.GetUsersPagedAsync(UserRole.Organizer, null, 1, 10);

        Assert.Equal(UserRole.Organizer, capturedRole);
    }

    [Fact]
    public async Task GetUsersPaged_SearchFilterIsPassedToRepository()
    {
        string? capturedSearch = null;
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.GetAllPagedAsync(null, It.IsAny<string?>(), 1, 10))
                .Callback<UserRole?, string?, int, int>((_, search, _, _) => capturedSearch = search)
                .ReturnsAsync((new List<User>(), 0));

        var svc = BuildService(userRepo);
        await svc.GetUsersPagedAsync(null, "alice", 1, 10);

        Assert.Equal("alice", capturedSearch);
    }

    [Fact]
    public async Task GetUsersPaged_NormalizesPageBelowOne()
    {
        int capturedPage = -1;
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.GetAllPagedAsync(null, null, It.IsAny<int>(), It.IsAny<int>()))
                .Callback<UserRole?, string?, int, int>((_, _, page, _) => capturedPage = page)
                .ReturnsAsync((new List<User>(), 0));

        var svc = BuildService(userRepo);
        await svc.GetUsersPagedAsync(null, null, page: 0, pageSize: 10);

        Assert.Equal(1, capturedPage);
    }

    [Fact]
    public async Task GetUsersPaged_NormalizesPageSizeAboveFifty()
    {
        int capturedPageSize = -1;
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.GetAllPagedAsync(null, null, It.IsAny<int>(), It.IsAny<int>()))
                .Callback<UserRole?, string?, int, int>((_, _, _, ps) => capturedPageSize = ps)
                .ReturnsAsync((new List<User>(), 0));

        var svc = BuildService(userRepo);
        await svc.GetUsersPagedAsync(null, null, page: 1, pageSize: 100);

        Assert.Equal(50, capturedPageSize);
    }

    [Fact]
    public async Task GetUserById_ReturnsNotFoundWhenUserDoesNotExist()
    {
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);

        var svc    = BuildService(userRepo);
        var result = await svc.GetUserByIdAsync(99);

        Assert.False(result.Succeeded);
        Assert.True(result.IsNotFound);
    }

    [Fact]
    public async Task GetUserById_ReturnsUserWhenFound()
    {
        var user     = MakeUser(5, "Charlie", "charlie@example.com", UserRole.Organizer);
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(user);

        var svc    = BuildService(userRepo);
        var result = await svc.GetUserByIdAsync(5);

        Assert.True(result.Succeeded);
        Assert.Equal(5, result.Data!.Id);
        Assert.Equal("Charlie", result.Data.FullName);
        Assert.Equal("Organizer", result.Data.Role);
    }

    // ── GetRegistrationsPagedAsync tests ──────────────────────────────────

    [Fact]
    public async Task GetRegistrationsPaged_ReturnsPagedResultFromRepository()
    {
        var regs    = new List<EventRegistration> { MakeRegistration(1), MakeRegistration(2, userId: 8) };
        var regRepo = new Mock<IRegistrationRepository>();
        regRepo.Setup(r => r.GetAllPagedAsync(null, null, null, 1, 10))
               .ReturnsAsync((regs, 2));

        var svc    = BuildService(regRepo: regRepo);
        var result = await svc.GetRegistrationsPagedAsync(null, null, null, 1, 10);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count());
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task GetRegistrationsPaged_EventIdFilterIsPassedToRepository()
    {
        int? capturedEventId = null;
        var regRepo = new Mock<IRegistrationRepository>();
        regRepo.Setup(r => r.GetAllPagedAsync(It.IsAny<int?>(), null, null, 1, 10))
               .Callback<int?, int?, RegistrationStatus?, int, int>((eid, _, _, _, _) => capturedEventId = eid)
               .ReturnsAsync((new List<EventRegistration>(), 0));

        var svc = BuildService(regRepo: regRepo);
        await svc.GetRegistrationsPagedAsync(eventId: 42, userId: null, status: null, page: 1, pageSize: 10);

        Assert.Equal(42, capturedEventId);
    }

    [Fact]
    public async Task GetRegistrationsPaged_UserIdFilterIsPassedToRepository()
    {
        int? capturedUserId = null;
        var regRepo = new Mock<IRegistrationRepository>();
        regRepo.Setup(r => r.GetAllPagedAsync(null, It.IsAny<int?>(), null, 1, 10))
               .Callback<int?, int?, RegistrationStatus?, int, int>((_, uid, _, _, _) => capturedUserId = uid)
               .ReturnsAsync((new List<EventRegistration>(), 0));

        var svc = BuildService(regRepo: regRepo);
        await svc.GetRegistrationsPagedAsync(eventId: null, userId: 7, status: null, page: 1, pageSize: 10);

        Assert.Equal(7, capturedUserId);
    }

    [Fact]
    public async Task GetRegistrationsPaged_StatusFilterIsPassedToRepository()
    {
        RegistrationStatus? capturedStatus = null;
        var regRepo = new Mock<IRegistrationRepository>();
        regRepo.Setup(r => r.GetAllPagedAsync(null, null, It.IsAny<RegistrationStatus?>(), 1, 10))
               .Callback<int?, int?, RegistrationStatus?, int, int>((_, _, s, _, _) => capturedStatus = s)
               .ReturnsAsync((new List<EventRegistration>(), 0));

        var svc = BuildService(regRepo: regRepo);
        await svc.GetRegistrationsPagedAsync(null, null, RegistrationStatus.Cancelled, 1, 10);

        Assert.Equal(RegistrationStatus.Cancelled, capturedStatus);
    }

    [Fact]
    public async Task GetRegistrationsPaged_NormalizesPageSizeBelowOne()
    {
        int capturedPageSize = -1;
        var regRepo = new Mock<IRegistrationRepository>();
        regRepo.Setup(r => r.GetAllPagedAsync(null, null, null, It.IsAny<int>(), It.IsAny<int>()))
               .Callback<int?, int?, RegistrationStatus?, int, int>((_, _, _, _, ps) => capturedPageSize = ps)
               .ReturnsAsync((new List<EventRegistration>(), 0));

        var svc = BuildService(regRepo: regRepo);
        await svc.GetRegistrationsPagedAsync(null, null, null, page: 1, pageSize: 0);

        Assert.Equal(10, capturedPageSize);
    }

    [Fact]
    public async Task GetRegistrationsPaged_ResponseIncludesUserEmailFromRegistration()
    {
        var reg = MakeRegistration(1);
        reg.User.Email = "attendee@test.com";
        var regRepo = new Mock<IRegistrationRepository>();
        regRepo.Setup(r => r.GetAllPagedAsync(null, null, null, 1, 10))
               .ReturnsAsync((new List<EventRegistration> { reg }, 1));

        var svc    = BuildService(regRepo: regRepo);
        var result = await svc.GetRegistrationsPagedAsync(null, null, null, 1, 10);

        Assert.Equal("attendee@test.com", result.Items.First().UserEmail);
    }
}
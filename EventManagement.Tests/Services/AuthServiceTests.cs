using EventManagement.API.DTOs.Auth;
using EventManagement.API.Entities;
using EventManagement.API.Enums;
using EventManagement.API.Repositories.Interfaces;
using EventManagement.API.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace EventManagement.Tests.Services;

public class AuthServiceTests
{
    private static IConfiguration BuildConfig() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"]              = "test-jwt-signing-key-that-is-at-least-32-chars!!",
                ["Jwt:Issuer"]           = "TestIssuer",
                ["Jwt:Audience"]         = "TestAudience",
                ["Jwt:ExpiresInMinutes"] = "60"
            })
            .Build();

    private static AuthService BuildService(Mock<IUserRepository> repo) =>
        new(repo.Object, BuildConfig());

    [Fact]
    public async Task Register_WithAdminRole_ReturnsFailure()
    {
        var repo = new Mock<IUserRepository>();
        var svc  = BuildService(repo);

        var result = await svc.RegisterAsync(new RegisterRequest
        {
            FullName = "Admin Test",
            Email    = "admin@test.com",
            Password = "Password1!",
            Role     = "Admin"
        });

        Assert.False(result.Succeeded);
        Assert.False(result.IsConflict);
        repo.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.EmailExistsAsync("dup@test.com")).ReturnsAsync(true);
        var svc = BuildService(repo);

        var result = await svc.RegisterAsync(new RegisterRequest
        {
            FullName = "Dup User",
            Email    = "dup@test.com",
            Password = "Password1!",
            Role     = "Organizer"
        });

        Assert.False(result.Succeeded);
        Assert.True(result.IsConflict);
        repo.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Register_ValidRequest_HashesPassword()
    {
        User? captured = null;
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        repo.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .Callback<User>(u => captured = u)
            .ReturnsAsync((User u) => u);
        var svc = BuildService(repo);

        var result = await svc.RegisterAsync(new RegisterRequest
        {
            FullName = "Test User",
            Email    = "user@test.com",
            Password = "Password1!",
            Role     = "Attendee"
        });

        Assert.True(result.Succeeded);
        Assert.NotNull(captured);
        Assert.NotEqual("Password1!", captured!.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("Password1!", captured.PasswordHash));
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsFailure()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByEmailAsync("user@test.com"))
            .ReturnsAsync(new User
            {
                Id           = 1,
                Email        = "user@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPass1!"),
                Role         = UserRole.Attendee
            });
        var svc = BuildService(repo);

        var result = await svc.LoginAsync(new LoginRequest
        {
            Email    = "user@test.com",
            Password = "WrongPassword1!"
        });

        Assert.False(result.Succeeded);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByEmailAsync("user@test.com"))
            .ReturnsAsync(new User
            {
                Id           = 1,
                FullName     = "Test User",
                Email        = "user@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password1!"),
                Role         = UserRole.Attendee
            });
        var svc = BuildService(repo);

        var result = await svc.LoginAsync(new LoginRequest
        {
            Email    = "user@test.com",
            Password = "Password1!"
        });

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.False(string.IsNullOrWhiteSpace(result.Data!.Token));
        Assert.Equal("Attendee", result.Data.Role);
        Assert.Equal(1, result.Data.UserId);
    }
}
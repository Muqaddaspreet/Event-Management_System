using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EventManagement.API.DTOs.Auth;
using EventManagement.API.Entities;
using EventManagement.API.Enums;
using EventManagement.API.Repositories.Interfaces;
using EventManagement.API.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace EventManagement.API.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IConfiguration _config;

    public AuthService(IUserRepository userRepo, IConfiguration config)
    {
        _userRepo = userRepo;
        _config = config;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        if (!Enum.TryParse<UserRole>(request.Role, out var role) || role == UserRole.Admin)
            return AuthResult.Failure("Role must be 'Organizer' or 'Attendee'.");

        if (await _userRepo.EmailExistsAsync(request.Email))
            return AuthResult.Conflict("Email is already in use.");

        var user = new User
        {
            FullName     = request.FullName,
            Email        = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role         = role,
            CreatedAt    = DateTime.UtcNow
        };

        await _userRepo.CreateAsync(user);

        return AuthResult.Ok(BuildResponse(user));
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return AuthResult.Failure("Invalid email or password.");

        return AuthResult.Ok(BuildResponse(user));
    }

    private AuthResponse BuildResponse(User user) => new()
    {
        Token    = GenerateToken(user),
        UserId   = user.Id,
        FullName = user.FullName,
        Email    = user.Email,
        Role     = user.Role.ToString()
    };

    private string GenerateToken(User user)
    {
        var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(
                         double.Parse(_config["Jwt:ExpiresInMinutes"] ?? "60"));

        var claims = new[]
        {
            new Claim("userId", user.Id.ToString()),
            new Claim("email",  user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("role",   user.Role.ToString()),
        };

        var token = new JwtSecurityToken(
            issuer:             _config["Jwt:Issuer"],
            audience:           _config["Jwt:Audience"],
            claims:             claims,
            expires:            expiry,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
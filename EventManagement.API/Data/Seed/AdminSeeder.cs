using EventManagement.API.Entities;
using EventManagement.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.API.Data.Seed;

public static class AdminSeeder
{
    public static async Task SeedAsync(AppDbContext db, IConfiguration configuration)
    {
        var fullName = configuration["AdminSeed:FullName"];
        var email    = configuration["AdminSeed:Email"];
        var password = configuration["AdminSeed:Password"];

        if (string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(email)    ||
            string.IsNullOrWhiteSpace(password))
        {
            Console.WriteLine("[AdminSeeder] Seed config not present — skipping.");
            return;
        }

        var adminExists = await db.Users.AnyAsync(u => u.Role == UserRole.Admin);
        if (adminExists)
        {
            Console.WriteLine("[AdminSeeder] Admin user already exists — skipping.");
            return;
        }

        var admin = new User
        {
            FullName     = fullName,
            Email        = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role         = UserRole.Admin,
            CreatedAt    = DateTime.UtcNow
        };

        db.Users.Add(admin);
        await db.SaveChangesAsync();

        Console.WriteLine($"[AdminSeeder] Admin user '{email}' created successfully.");
    }
}
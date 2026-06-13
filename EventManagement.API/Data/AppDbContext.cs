using Microsoft.EntityFrameworkCore;
using EventManagement.API.Entities;
using EventManagement.API.Enums;

namespace EventManagement.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventRegistration> EventRegistrations => Set<EventRegistration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Users ──────────────────────────────────────────────────────
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            e.Property(u => u.Email).IsRequired().HasMaxLength(150);
            e.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
            e.Property(u => u.Role)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);
            e.Property(u => u.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            e.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");
        });

        // ── Categories ────────────────────────────────────────────────
        modelBuilder.Entity<Category>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).IsRequired().HasMaxLength(100);

            e.HasIndex(c => c.Name)
                .IsUnique()
                .HasDatabaseName("IX_Categories_Name");
        });

        // ── Venues ────────────────────────────────────────────────────
        modelBuilder.Entity<Venue>(e =>
        {
            e.HasKey(v => v.Id);
            e.Property(v => v.Name).IsRequired().HasMaxLength(150);
            e.Property(v => v.Address).IsRequired().HasMaxLength(300);
            e.Property(v => v.City).IsRequired().HasMaxLength(100);
            e.Property(v => v.Capacity).IsRequired();

            e.ToTable(t => t.HasCheckConstraint("CK_Venues_Capacity", "`Capacity` > 0"));
        });

        // ── Events ────────────────────────────────────────────────────
        modelBuilder.Entity<Event>(e =>
        {
            e.HasKey(ev => ev.Id);
            e.Property(ev => ev.Title).IsRequired().HasMaxLength(200);
            e.Property(ev => ev.Description).HasColumnType("longtext");
            e.Property(ev => ev.StartTime).IsRequired();
            e.Property(ev => ev.EndTime).IsRequired();
            e.Property(ev => ev.Capacity).IsRequired();
            e.Property(ev => ev.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(EventStatus.PendingApproval);
            e.Property(ev => ev.OrganizerId).IsRequired();
            e.Property(ev => ev.CategoryId).IsRequired();
            e.Property(ev => ev.VenueId).IsRequired();
            e.Property(ev => ev.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            e.Property(ev => ev.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            e.ToTable(t => t.HasCheckConstraint("CK_Events_Capacity", "`Capacity` > 0"));

            e.HasOne(ev => ev.Organizer)
                .WithMany(u => u.OrganizedEvents)
                .HasForeignKey(ev => ev.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(ev => ev.Category)
                .WithMany(c => c.Events)
                .HasForeignKey(ev => ev.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(ev => ev.Venue)
                .WithMany(v => v.Events)
                .HasForeignKey(ev => ev.VenueId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(ev => ev.OrganizerId).HasDatabaseName("IX_Events_OrganizerId");
            e.HasIndex(ev => ev.Status).HasDatabaseName("IX_Events_Status");
            e.HasIndex(ev => ev.CategoryId).HasDatabaseName("IX_Events_CategoryId");
            e.HasIndex(ev => ev.VenueId).HasDatabaseName("IX_Events_VenueId");
        });

        // ── EventRegistrations ────────────────────────────────────────
        modelBuilder.Entity<EventRegistration>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.EventId).IsRequired();
            e.Property(r => r.UserId).IsRequired();
            e.Property(r => r.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(RegistrationStatus.Registered);
            e.Property(r => r.RegisteredAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            e.Property(r => r.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            e.HasIndex(r => new { r.EventId, r.UserId })
                .IsUnique()
                .HasDatabaseName("UQ_EventRegistrations_EventId_UserId");

            e.HasIndex(r => r.UserId)
                .HasDatabaseName("IX_EventRegistrations_UserId");

            e.HasOne(r => r.Event)
                .WithMany(ev => ev.Registrations)
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.User)
                .WithMany(u => u.Registrations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
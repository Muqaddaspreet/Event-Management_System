namespace EventManagement.API.DTOs.Admin;

public class AdminDashboardResponse
{
    public int TotalUsers { get; init; }
    public int TotalEvents { get; init; }
    public int PendingEvents { get; init; }
    public int PublishedEvents { get; init; }
    public int TotalRegistrations { get; init; }
}
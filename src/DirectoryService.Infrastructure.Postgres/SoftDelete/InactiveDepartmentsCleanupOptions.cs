namespace DirectoryService.Infrastructure.Postgres.SoftDelete;

public record InactiveDepartmentsCleanupOptions
{
    public TimeSpan Interval { get; init; }
}
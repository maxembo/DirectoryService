namespace DirectoryService.Infrastructure.Postgres.SoftDelete;

public class InactiveDepartmentsCleanupOptions
{
    public TimeSpan Interval { get; init; }
}
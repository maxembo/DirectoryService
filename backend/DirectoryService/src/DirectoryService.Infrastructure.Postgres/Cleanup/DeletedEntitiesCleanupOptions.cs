namespace DirectoryService.Infrastructure.Postgres.Cleanup;

public record DeletedEntitiesCleanupOptions
{
    public TimeSpan Interval { get; init; }
}
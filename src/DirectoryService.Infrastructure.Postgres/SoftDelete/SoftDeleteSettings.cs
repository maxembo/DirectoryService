namespace DirectoryService.Infrastructure.Postgres.SoftDelete;

public class SoftDeleteSettings
{
    public int Years { get; init; }

    public int Months { get; init; }

    public int Days { get; init; }

    public int Hours { get; init; }

    public int Minutes { get; init; }

    public int Seconds { get; init; }
}
namespace DirectoryService.Contracts.Search.GlobalSearch;

public record GlobalSearchDto
{
    public Guid Id { get; init; }

    public string Type { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string SubTitle { get; init; } = string.Empty;

    public bool IsActive { get; init; } = true;
    
    public DateTime? DeletedAt { get; init; }
}
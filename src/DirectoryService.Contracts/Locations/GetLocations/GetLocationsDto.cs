namespace DirectoryService.Contracts.Locations.GetLocations;

public record GetLocationsDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Timezone { get; init; } = string.Empty;

    public bool IsActive { get; init; } = true;

    public required AddressDto Address { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }
}
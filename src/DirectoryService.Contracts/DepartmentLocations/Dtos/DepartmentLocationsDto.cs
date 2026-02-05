namespace DirectoryService.Contracts.DepartmentLocations.Dtos;

public record DepartmentLocationsDto
{
    public Guid Id { get; init; }

    public Guid DepartmentId { get; init; }

    public Guid LocationId { get; init; }
}
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;

namespace DirectoryService.Domain.Locations;

public class Location : Shared.Entity<LocationId>
{
    // ef core
    private Location(LocationId id)
        : base(id)
    { }

    private Location(LocationId id, LocationName name, Timezone timezone, Address address)
        : base(id)
    {
        Name = name;
        Timezone = timezone;
        Address = address;
    }

    private readonly List<Department> _departments = [];

    public IReadOnlyList<Department> Departments => _departments.AsReadOnly();

    public LocationName Name { get; private set; } = null!;

    public Timezone Timezone { get; private set; } = null!;

    public bool IsActive { get; private set; } = true;

    public Address Address { get; private set; } = null!;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public static Result<Location> Create(
        LocationId locationId, LocationName locationName, Timezone timezone, Address address)
    {
        var location = new Location(locationId, locationName, timezone, address);

        return Result.Success(location);
    }
}
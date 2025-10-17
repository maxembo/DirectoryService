using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;

namespace DirectoryService.Domain.Locations;

public class Location : Shared.Entity<LocationId>
{
    // ef core
    private Location(LocationId locationId)
        : base(locationId)
    {
    }

    private Location(LocationId locationId, LocationName locationName, Timezone timezone, Address address)
        : base(locationId)
    {
        LocationName = locationName;
        Timezone = timezone;
        Address = address;
    }

    private readonly List<Department> _departments = [];

    public IReadOnlyList<Department> Departments => _departments.AsReadOnly();

    public LocationName LocationName { get; private set; }

    public Timezone Timezone { get; private set; }

    public bool IsActive { get; private set; } = true;

    public Address Address { get; private set; }

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public static Result<Location> Create(
        LocationId locationId, LocationName locationName, Timezone timezone, Address address)
    {
        var location = new Location(locationId, locationName, timezone, address);

        return Result.Success(location);
    }
}
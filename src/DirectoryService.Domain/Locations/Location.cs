using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;

namespace DirectoryService.Domain.Locations;

public sealed class Location : Shared.Entity<LocationId>
{
    public Location(LocationId id, LocationName name, Timezone timezone, Address address)
        : base(id)
    {
        Name = name;
        Timezone = timezone;
        Address = address;
    }

    // ef core
    private Location(LocationId id)
        : base(id)
    { }

    private readonly List<DepartmentLocation> _departmentLocations = [];

    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations.AsReadOnly();

    public LocationName Name { get; private set; } = null!;

    public Timezone Timezone { get; private set; } = null!;

    public bool IsActive { get; private set; } = true;

    public Address Address { get; private set; } = null!;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
}
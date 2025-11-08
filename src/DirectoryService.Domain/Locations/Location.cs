using DirectoryService.Domain.Departments;

namespace DirectoryService.Domain.Locations;

public class Location : Shared.Entity<LocationId>
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
    {
    }

    private readonly List<Department> _departments = [];

    public IReadOnlyList<Department> Departments => _departments.AsReadOnly();

    public LocationName Name { get; private set; } = null!;

    public Timezone Timezone { get; private set; } = null!;

    public bool IsActive { get; private set; } = true;

    public Address Address { get; private set; } = null!;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
}
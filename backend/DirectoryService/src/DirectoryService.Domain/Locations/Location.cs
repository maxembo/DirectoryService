using DirectoryService.Domain.DepartmentLocations;
using SharedService.SharedKernel;

namespace DirectoryService.Domain.Locations;

public sealed class Location : BaseEntity<LocationId>, ISoftDeletable
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

    private readonly List<DepartmentLocation> _departments = [];

    public IReadOnlyList<DepartmentLocation> Departments => _departments.AsReadOnly();

    public LocationName Name { get; private set; } = null!;

    public Timezone Timezone { get; private set; } = null!;

    public bool IsActive { get; private set; } = true;

    public Address Address { get; private set; } = null!;

    public DateTime? DeletedAt { get; private set; }

    public DeletionReason? DeletionReason { get; private set; }

    public void MarkAsDelete()
    {
        ApplySoftDelete(Domain.DeletionReason.MANUAL);
    }

    public void Update(LocationName name, Timezone timezone, Address address)
    {
        Name = name;
        Address = address;
        Timezone = timezone;

        UpdatedAt = DateTime.UtcNow;
    }

    public void Restore()
    {
        IsActive = true;
        DeletedAt = null;
        DeletionReason = null;

        UpdatedAt = DateTime.UtcNow;
    }

    private void ApplySoftDelete(DeletionReason reason)
    {
        var now = DateTime.UtcNow;

        IsActive = false;
        DeletedAt = now;
        DeletionReason = reason;

        UpdatedAt = now;
    }
}
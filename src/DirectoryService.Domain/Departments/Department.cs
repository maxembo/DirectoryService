using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using Shared;

namespace DirectoryService.Domain.Departments;

public sealed class Department : Shared.Entity<DepartmentId>
{
    // ef core
    private Department(DepartmentId id)
        : base(id)
    { }

    public Department(
        DepartmentId id,
        DepartmentName name,
        Identifier identifier,
        Path path,
        IEnumerable<DepartmentLocation> locations,
        DepartmentId? parentId = null)
        : base(id)
    {
        Name = name;
        Identifier = identifier;
        ParentId = parentId;
        Path = path;
        _locations = locations.ToList();
    }

    private readonly List<DepartmentLocation> _locations = [];

    private readonly List<Department> _childrens = [];

    private readonly List<DepartmentPosition> _positions = [];

    public IReadOnlyList<DepartmentLocation> Locations => _locations.AsReadOnly();

    public IReadOnlyList<Department> Childrens => _childrens.AsReadOnly();

    public IReadOnlyList<DepartmentPosition> Positions => _positions.AsReadOnly();

    public DepartmentName Name { get; private set; } = null!;

    public Identifier Identifier { get; private set; } = null!;

    public DepartmentId? ParentId { get; private set; }

    public Path Path { get; private set; } = null!;

    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public UnitResult<Error> Rename(string name)
    {
        var nameResult = DepartmentName.Create(name);

        if (nameResult.IsFailure)
            return GeneralErrors.Invalid("department rename");

        Name = nameResult.Value;

        return UnitResult.Success<Error>();
    }

    public static Result<Department, Error> CreateParent(
        DepartmentName name,
        Identifier identifier,
        IEnumerable<DepartmentLocation> locations,
        DepartmentId? departmentId = null)
    {
        var locationsList = locations.ToList();
        if (locationsList.Count == 0)
            return GeneralErrors.Required("locations");

        var path = Path.CreateParent(identifier);

        return new Department(departmentId ?? DepartmentId.CreateNew(), name, identifier, path, locationsList);
    }

    public static Result<Department, Error> CreateChild(
        DepartmentName name,
        Identifier identifier,
        Department parent,
        IEnumerable<DepartmentLocation> locations,
        DepartmentId? departmentId = null)
    {
        var locationsList = locations.ToList();
        if (locationsList.Count == 0)
            return GeneralErrors.Required("locations");

        var path = parent.Path.CreateChild(identifier);

        return new Department(
            departmentId ?? DepartmentId.CreateNew(), name, identifier, path, locationsList, parent.Id);
    }

    // public UnitResult<Error> SetPath(Identifier identifier, Path? path = null)
    // {
    //     if (path == null)
    //     {
    //         var resultPath = Path.Create(identifier.Value);
    //
    //         if (resultPath.IsFailure)
    //             return GeneralErrors.Invalid("department path");
    //
    //         Path = resultPath.Value;
    //     }
    //
    //     var resultChildPath = Path.Child(identifier.Value);
    //
    //     if (resultChildPath.IsFailure)
    //         return GeneralErrors.Invalid("department path");
    //
    //     Path = resultChildPath.Value;
    //
    //     return UnitResult.Success<Error>();
    // }

    public UnitResult<Error> AddLocation(DepartmentLocation location)
    {
        if (_locations.Contains(location))
            return GeneralErrors.Invalid("department location");

        _locations.Add(location);

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> RemoveLocation(DepartmentLocation location)
    {
        if (!_locations.Contains(location))
            return GeneralErrors.Invalid("department location");

        _locations.Remove(location);

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> AddChild(Department department)
    {
        if (_childrens.Contains(department))
            return GeneralErrors.Invalid("department child");

        _childrens.Add(department);

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> RemoveChild(Department department)
    {
        if (!_childrens.Contains(department))
            return GeneralErrors.Invalid("department child");

        _childrens.Remove(department);

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> AddPosition(DepartmentPosition position)
    {
        if (_positions.Contains(position))
            return GeneralErrors.Invalid("department position");

        _positions.Add(position);

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> RemovePosition(DepartmentPosition position)
    {
        if (!_positions.Contains(position))
            return GeneralErrors.Invalid("department position");

        _positions.Remove(position);

        return UnitResult.Success<Error>();
    }

    public void MarkAsDelete()
    {
        IsActive = false;
    }
}
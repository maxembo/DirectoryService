using CSharpFunctionalExtensions;
using Shared;

namespace DirectoryService.Domain.Departments;

public class Department : Shared.Entity<DepartmentId>
{
    // ef core
    private Department(DepartmentId id)
        : base(id)
    {
    }

    public Department(
        DepartmentId id,
        DepartmentName name,
        Identifier identifier,
        DepartmentId? parentId,
        Path path)
        : base(id)
    {
        Name = name;
        Identifier = identifier;
        ParentId = parentId;
        Path = path;
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

    public Department Parent { get; private set; } = null!;

    public Path Path { get; private set; } = null!;

    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public UnitResult<Error> Rename(string name)
    {
        var nameResult = DepartmentName.Create(name);

        if (nameResult.IsFailure)
            return nameResult.Error;

        Name = nameResult.Value;

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> SetPath(Identifier identifier, Path? path = null)
    {
        if (path == null)
        {
            var resultPath = Path.Create(identifier.Value);

            if (resultPath.IsFailure)
                return resultPath.Error;

            Path = resultPath.Value;
        }

        var resultChildPath = Path.Child(identifier.Value);

        if (resultChildPath.IsFailure)
            return resultChildPath.Error;

        Path = resultChildPath.Value;

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> AddLocation(DepartmentLocation location)
    {
        if (_locations.Contains(location))
            return GeneralErrors.ValueIsInvalid("deparment");

        _locations.Add(location);

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> RemoveLocation(DepartmentLocation location)
    {
        if (!_locations.Contains(location))
            return GeneralErrors.ValueIsInvalid("department");

        _locations.Remove(location);

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> AddChild(Department department)
    {
        if (_childrens.Contains(department))
            return GeneralErrors.ValueIsInvalid("department");

        _childrens.Add(department);

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> RemoveChild(Department department)
    {
        if (!_childrens.Contains(department))
            return GeneralErrors.ValueIsInvalid("department");

        _childrens.Remove(department);

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> AddPosition(DepartmentPosition position)
    {
        if (_positions.Contains(position))
            return GeneralErrors.ValueIsInvalid("department");

        _positions.Add(position);

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> RemovePosition(DepartmentPosition position)
    {
        if (!_positions.Contains(position))
            return GeneralErrors.ValueIsInvalid("department");

        _positions.Remove(position);

        return UnitResult.Success<Error>();
    }

    public void MarkAsDelete()
    {
        IsActive = false;
    }
}
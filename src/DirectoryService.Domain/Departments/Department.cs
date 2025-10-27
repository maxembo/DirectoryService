using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Departments;

public class Department : Shared.Entity<DepartmentId>
{
    // ef core
    private Department(DepartmentId id)
        : base(id)
    { }

    private Department(
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

    public static Result<Department> Create(
        DepartmentId departmentId,
        DepartmentName departmentName,
        Identifier identifier,
        DepartmentId? parentId,
        Path path)
    {
        var departments = new Department(departmentId, departmentName, identifier, parentId, path);

        return Result.Success(departments);
    }

    public Result Rename(string name)
    {
        var nameResult = DepartmentName.Create(name);

        if (nameResult.IsFailure)
            return Result.Failure(nameResult.Error);

        Name = nameResult.Value;

        return Result.Success();
    }

    public Result SetPath(Identifier identifier, Path? path = null)
    {
        if (path == null)
        {
            var resultPath = Path.Create(identifier.Value);

            if (resultPath.IsFailure)
                return Result.Failure(resultPath.Error);

            Path = resultPath.Value;
            return Result.Success();
        }

        var resultChildPath = Path.Child(identifier.Value);

        if (resultChildPath.IsFailure)
            return Result.Failure(resultChildPath.Error);

        Path = resultChildPath.Value;

        return Result.Success();
    }

    public Result AddLocation(DepartmentLocation location)
    {
        if (_locations.Contains(location))
            return Result.Failure($"Location {location} is already added.");

        _locations.Add(location);

        return Result.Success();
    }

    public Result RemoveLocation(DepartmentLocation location)
    {
        if (!_locations.Contains(location))
            return Result.Failure($"Location {location} is not added.");

        _locations.Remove(location);

        return Result.Success();
    }

    public Result AddChild(Department department)
    {
        if (_childrens.Contains(department))
            return Result.Failure($"Department {department} is already added.");

        _childrens.Add(department);

        return Result.Success();
    }

    public Result RemoveChild(Department department)
    {
        if (!_childrens.Contains(department))
            return Result.Failure($"Department {department} is not added.");

        _childrens.Remove(department);

        return Result.Success();
    }

    public Result AddPosition(DepartmentPosition position)
    {
        if (_positions.Contains(position))
            return Result.Failure($"Position {position} is already added.");

        _positions.Add(position);

        return Result.Success();
    }

    public Result RemovePosition(DepartmentPosition position)
    {
        if (!_positions.Contains(position))
            return Result.Failure($"Position {position} is not added.");

        _positions.Remove(position);

        return Result.Success();
    }

    public void MarkAsDelete()
    {
        IsActive = false;
    }
}
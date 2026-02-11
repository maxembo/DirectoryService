namespace DirectoryService.Contracts.Departments.GetDepartments.Dtos;

public record GetDepartmentDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Identifier { get; init; } = string.Empty;

    public Guid? ParentId { get; init; }

    public string Path { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public int Depth { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }

    public bool HasMoreChildren { get; init; }

    public List<Guid> Childrens { get; init; } = [];
}
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments.Dtos;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;

namespace DirectoryService.Application.Departments.Queries;

public class GetTopFiveDepartmentsWithMostPositionsHandler : IQueryHandler<GetDepartmentDto[]>
{
    private const int TOP_DEPARTMENTS_LIMIT = 5;

    private readonly IReadDbContext _readDbContext;

    public GetTopFiveDepartmentsWithMostPositionsHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<GetDepartmentDto[]> Handle(CancellationToken cancellationToken)
    {
        var departmentsQuery = _readDbContext.DepartmentsRead;

        var departments = await departmentsQuery
            .Select(
                d => new GetDepartmentDto
                {
                    Id = d.Id.Value,
                    ParentId = d.ParentId!.Value,
                    Name = d.Name.Value,
                    Identifier = d.Identifier.Value,
                    IsActive = d.IsActive,
                    Path = d.Path.Value,
                    Depth = d.Path.Depth,
                    CreatedAt = d.CreatedAt,
                    UpdatedAt = d.UpdatedAt,
                    PositionCount = d.Positions.Count,
                    Locations = d.Locations.Select(l => l.LocationId.Value).ToList(),
                })
            .OrderByDescending(d => d.PositionCount)
            .Take(TOP_DEPARTMENTS_LIMIT)
            .ToListAsync(cancellationToken);

        return departments.ToArray();
    }
}
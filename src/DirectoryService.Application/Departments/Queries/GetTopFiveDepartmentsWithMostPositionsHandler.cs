using DirectoryService.Application.Database;
using DirectoryService.Contracts.DepartmentLocations.Dtos;
using DirectoryService.Contracts.Departments.Dtos;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;

namespace DirectoryService.Application.Departments.Queries;

public class GetTopFiveDepartmentsWithMostPositionsHandler : IQueryHandler<GetDepartmentDto[]>
{
    private const int LIMIT = 100;

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
                    Childrens = d.Childrens.Select(c => c.Id.Value).ToList(),
                    PositionCount = d.Positions.Count,
                    Locations = d.Locations.Select(
                        dl => new DepartmentLocationsDto
                        {
                            Id = dl.Id.Value,
                            LocationId = dl.LocationId.Value,
                            DepartmentId = dl.DepartmentId.Value,
                        }).ToList(),
                })
            .OrderByDescending(d => d.PositionCount)
            .Take(LIMIT)
            .ToListAsync(cancellationToken);

        return departments.ToArray();
    }
}
using DirectoryService.Application.Constants;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments.GetTopFiveDepartmentsWithMostPositions.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using SharedService.Core.Abstractions;

namespace DirectoryService.Application.Departments.Queries.GetTopFiveDepartmentsWithMostPositions;

public class GetTopFiveDepartmentsWithMostPositionsHandler : IQueryHandler<GetDepartmentDto[]>
{
    private const int TOP_DEPARTMENTS_LIMIT = 5;

    private readonly IReadDbContext _readDbContext;
    private readonly HybridCache _cache;

    public GetTopFiveDepartmentsWithMostPositionsHandler(IReadDbContext readDbContext, HybridCache cache)
    {
        _readDbContext = readDbContext;
        _cache = cache;
    }

    public async Task<GetDepartmentDto[]> Handle(CancellationToken cancellationToken)
    {
        return await GetPresignedTopFiveDepartmentsWithPositionsFromCache(cancellationToken);
    }

    private async Task<GetDepartmentDto[]> GetPresignedTopFiveDepartmentsWithPositionsFromCache(
        CancellationToken cancellationToken)
    {
        string key = CacheKeys.CreateDepartmentsKey("top", TOP_DEPARTMENTS_LIMIT.ToString());

        return await _cache.GetOrCreateAsync(
            key,
            factory: async ct => await GetTopFiveDepartmentsWithPositions(ct),
            tags: [CacheKeys.DEPARTMENT_KEY],
            cancellationToken: cancellationToken);
    }

    private async Task<GetDepartmentDto[]> GetTopFiveDepartmentsWithPositions(CancellationToken cancellationToken)
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
using Dapper;
using DirectoryService.Application.Constants;
using DirectoryService.Contracts.Departments.GetTopFiveDepartmentsWithMostPositions.Dtos;
using Microsoft.Extensions.Caching.Hybrid;
using Shared.Core.Abstractions;
using Shared.Core.Database;

namespace DirectoryService.Application.Departments.Queries.GetTopFiveDepartmentsWithMostPositions;

public class GetTopFiveDepartmentsWithMostPositionsHandlerDapper : IQueryHandler<GetDepartmentDto[]>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly HybridCache _cache;

    public GetTopFiveDepartmentsWithMostPositionsHandlerDapper(
        IDbConnectionFactory dbConnectionFactory, HybridCache cache)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _cache = cache;
    }

    public async Task<GetDepartmentDto[]> Handle(CancellationToken cancellationToken)
    {
        return await GetPresignedTopFiveDepartmentsWithPositionsFromCache(cancellationToken);
    }

    private async Task<GetDepartmentDto[]> GetPresignedTopFiveDepartmentsWithPositionsFromCache(
        CancellationToken cancellationToken)
    {
        string key = CacheKeys.CreateDepartmentsKey("top_5");

        return await _cache.GetOrCreateAsync(
            key,
            factory: async ct => await GetTopFiveDepartmentsWithPositions(ct),
            cancellationToken: cancellationToken);
    }

    private async Task<GetDepartmentDto[]> GetTopFiveDepartmentsWithPositions(CancellationToken cancellationToken)
    {
        var dbConnection = _dbConnectionFactory.GetDbConnection();

        const string sql = """
                           SELECT d.id,
                                  d.parent_id,
                                  d.name,
                                  d.identifier,
                                  d.path,
                                  d.is_active,
                                  d.created_at,
                                  d.updated_at,
                                  t.positions_count,
                                  array_agg(dl.location_id) AS locations
                           FROM departments d
                                    JOIN (SELECT dp.department_id AS id,
                                                 COUNT(dp.id)     AS positions_count
                                          FROM department_positions dp
                                          GROUP BY dp.department_id
                                          ORDER BY positions_count DESC
                                          LIMIT 5) t ON t.id = d.id
                                    LEFT JOIN department_locations dl ON dl.department_id = d.id
                           GROUP BY d.id, t.positions_count
                           ORDER BY t.positions_count DESC;
                           """;

        var departments = await dbConnection
            .QueryAsync<GetDepartmentDto, long, Guid[], GetDepartmentDto>(
                sql, splitOn: "positions_count, locations", map:
                (department, count, locations)
                    => department with { PositionCount = count, Locations = locations.ToList() });

        return departments.ToArray();
    }
}
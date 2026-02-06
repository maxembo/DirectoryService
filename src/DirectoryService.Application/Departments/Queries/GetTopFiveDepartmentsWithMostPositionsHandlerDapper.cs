using Dapper;
using DirectoryService.Contracts.DepartmentLocations.Dtos;
using DirectoryService.Contracts.Departments.Dtos;
using Shared.Abstractions;
using Shared.Database;

namespace DirectoryService.Application.Departments.Queries;

public class GetTopFiveDepartmentsWithMostPositionsHandlerDapper : IQueryHandler<GetDepartmentDto[]>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetTopFiveDepartmentsWithMostPositionsHandlerDapper(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<GetDepartmentDto[]> Handle(CancellationToken cancellationToken)
    {
        var dbConnection = _dbConnectionFactory.GetDbConnection();

        const string sql = """
                           WITH top_depts AS (
                               SELECT
                                   d.id            AS Id,
                                   d.parent_id     AS ParentId,
                                   d.is_active     AS IsActive,
                                   d.created_at    AS CreatedAt,
                                   d.updated_at    AS UpdatedAt,
                                   d.identifier    AS Identifier,
                                   d.name          AS Name,
                                   d.depth         AS Depth,
                                   d.path          AS Path,
                                   COUNT(dp.id)    AS PositionCount
                               FROM departments d
                                        LEFT JOIN department_positions dp ON dp.department_id = d.id
                               GROUP BY d.id
                               ORDER BY PositionCount DESC
                               LIMIT 5
                           )
                           SELECT
                               td.Id,
                               td.ParentId,
                               td.IsActive,
                               td.CreatedAt,
                               td.UpdatedAt,
                               td.Identifier,
                               td.Name,
                               td.Depth,
                               td.Path,
                               COALESCE((SELECT array_agg(ch.id) FROM departments ch WHERE ch.parent_id = td.id), ARRAY[]::uuid[]) AS childrens_ids,
                               dl.id          AS Id,
                               dl.department_id AS DepartmentId,
                               dl.location_id   AS LocationId,
                               td.PositionCount
                           FROM top_depts td
                                    LEFT JOIN department_locations dl ON dl.department_id = td.Id
                           ORDER BY td.PositionCount DESC, td.Name;
                           """;

        var lookup = new Dictionary<Guid, GetDepartmentDto>();

        await dbConnection
            .QueryAsync<GetDepartmentDto, DepartmentLocationsDto, long, GetDepartmentDto>(
                sql,
                splitOn: "Id,PositionCount",
                map: (dept, loc, count) =>
                {
                    if (!lookup.TryGetValue(dept.Id, out var existing))
                    {
                        existing = dept with { PositionCount = count, Locations = new List<DepartmentLocationsDto>(), };
                        lookup.Add(dept.Id, existing);
                    }

                    if (loc.Id != Guid.Empty)
                    {
                        existing.Locations.Add(loc);
                    }

                    return existing;
                });
        return lookup.Values.ToArray();
    }
}
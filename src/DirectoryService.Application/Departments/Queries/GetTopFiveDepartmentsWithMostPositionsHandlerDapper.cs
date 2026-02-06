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
                               
                               dl.id                 AS LocationDepartmentId,
                           
                                 dl.id               AS Id,
                                 dl.department_id    AS DepartmentId,
                                 dl.location_id      AS LocationId,
                               
                               COALESCE((SELECT array_agg(ch.id) FROM departments ch WHERE ch.parent_id = td.id), 
                               ARRAY[]::uuid[]) AS children_ids,
                               
                               td.PositionCount
                           FROM top_depts td
                                    LEFT JOIN department_locations dl ON dl.department_id = td.Id
                           ORDER BY td.PositionCount DESC, td.Name;
                           """;

        var lookup = new Dictionary<Guid, GetDepartmentDto>();

        await dbConnection
            .QueryAsync<GetDepartmentDto, DepartmentLocationsDto, Guid[], long, GetDepartmentDto>(
                sql,
                splitOn: "LocationDepartmentId, children_ids, PositionCount",
                map: (dept, dl, childrens, count) =>
                {
                    if (!lookup.TryGetValue(dept.Id, out var existing))
                    {
                        existing = dept with
                        {
                            PositionCount = count,
                            Locations = new List<DepartmentLocationsDto>(),
                            Childrens = new List<Guid>()
                        };
                        lookup.Add(dept.Id, existing);
                    }

                    existing.Locations.Add(dl);
                    foreach (var ch in childrens)
                    {
                        if (!existing.Childrens.Contains(ch))
                            existing.Childrens.Add(ch);
                    }

                    return existing;
                });
        return lookup.Values.ToArray();
    }
}
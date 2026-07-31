using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Contracts.Positions.GetPositions;
using SharedService.Core.Abstractions;
using SharedService.Core.Database;
using SharedService.SharedKernel;
using SharedService.SharedKernel.Response;

namespace DirectoryService.Application.Positions.Queries.GetPositions;

public class GetPositionsHandler : IQueryHandler<PaginationEnvelope<GetPositionsDto>, GetPositionsQuery>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetPositionsHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Result<PaginationEnvelope<GetPositionsDto>, Errors>> Handle(
        GetPositionsQuery query, CancellationToken cancellationToken)
    {
        var request = query.Request;

        var parameters = new DynamicParameters();
        var conditions = new List<string>();

        if (request.DepartmentsIds is { Length: > 0 } departmentIds)
        {
            parameters.Add("@departmentIds", departmentIds, DbType.Object);
            conditions.Add(
                """
                EXISTS
                (SELECT 1
                 FROM department_positions dp
                 WHERE dp.position_id = p.id
                   AND dp.department_id = ANY (@departmentIds))
                """);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            parameters.Add("@search", request.Search, DbType.String);
            conditions.Add("p.name ILIKE '%' || @search || '%'");
        }

        if (request.IsActive.HasValue)
        {
            parameters.Add("@isActive", request.IsActive, DbType.Boolean);
            conditions.Add("p.is_active = @isActive");
        }

        parameters.Add("@page", (request.Page - 1) * request.PageSize, DbType.Int32);
        parameters.Add("@pageSize", request.PageSize, DbType.Int32);

        string whereClause =
            conditions.Count > 0
                ? "WHERE " + string.Join(" AND ", conditions)
                : string.Empty;

        string sortBy = request.SortBy?.ToLowerInvariant() switch
        {
            "name" => "p.name",
            "created" => "p.created_at",
            "updated" => "p.updated_at",
            "status" => "p.is_active",
            "department_count" => "department_count",
            _ => "p.name",
        };

        string sortDirection = query.Request.SortDirection?.ToLowerInvariant() == "asc"
            ? "ASC"
            : "DESC";

        string orderClause = $"ORDER BY {sortBy} {sortDirection}, p.id {sortDirection}";

        var connection = _dbConnectionFactory.GetDbConnection();

        long totalCount = 0;

        var positions = await connection.QueryAsync<GetPositionsDto, long, GetPositionsDto>(
            $"""
             SELECT p.id,
                    p.name,
                    p.description,
                    p.is_active,
                    p.created_at,
                    p.updated_at,
                    p.deleted_at,
                    (SELECT COUNT(*)
                     FROM department_positions dp
                              JOIN departments d ON d.id = dp.department_id
                     WHERE dp.position_id = p.id
                       AND d.is_active = true) AS department_count,
                    COUNT(*) OVER ()           as total_count
             FROM positions p 
             {whereClause}
             {orderClause}
             LIMIT @pageSize OFFSET @page
             """,
            splitOn: "total_count",
            map: (position, count) =>
            {
                totalCount = count;

                return position;
            },
            param: parameters);

        return new PaginationEnvelope<GetPositionsDto>(
            positions.ToList(), totalCount, request.Page, request.PageSize);
    }
}
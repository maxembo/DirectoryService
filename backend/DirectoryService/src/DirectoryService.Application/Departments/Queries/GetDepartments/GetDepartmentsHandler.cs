using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Contracts.Departments.GetDepartments.Dtos;
using SharedService.Core.Abstractions;
using SharedService.Core.Database;
using SharedService.SharedKernel;
using SharedService.SharedKernel.Response;

namespace DirectoryService.Application.Departments.Queries.GetDepartments;

public class GetDepartmentsHandler : IQueryHandler<PaginationEnvelope<DepartmentShortDto>, GetDepartmentsQuery>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetDepartmentsHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Result<PaginationEnvelope<DepartmentShortDto>, Errors>> Handle(
        GetDepartmentsQuery query, CancellationToken cancellationToken)
    {
        var parameters = new DynamicParameters();
        var conditions = new List<string>();

        var request = query.Request;

        if (request.LocationIds is { Length: > 0 } locationIds)
        {
            parameters.Add("locationIds", locationIds, DbType.Object);
            conditions.Add(
                """
                EXISTS
                (SELECT 1
                 FROM department_locations dl
                 WHERE dl.department_id = d.id
                   AND dl.location_id = ANY (@locationIds)
                   )
                """);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            parameters.Add("search", request.Search, DbType.String);
            conditions.Add("d.name ILIKE '%' || @search || '%'");
        }

        if (request.IsActive.HasValue)
        {
            parameters.Add("isActive", request.IsActive, DbType.Boolean);
            conditions.Add("d.is_active = @isActive");
        }

        if (request.IsArchived.HasValue)
        {
            conditions.Add(request.IsArchived.Value ? "d.deleted_at IS NOT NULL" : "d.deleted_at IS NULL");
        }

        if (request.ParentId.HasValue)
        {
            parameters.Add("parentId", request.ParentId, DbType.Guid);
            conditions.Add("d.parent_id = @parentId");
        }

        if (request.IsParent.HasValue)
        {
            conditions.Add(
                $"(SELECT COUNT(*) FROM departments WHERE parent_id = d.id) {(request.IsParent.Value ? "> 0" : "= 0")}");
        }

        parameters.Add("page", (request.Page - 1) * request.PageSize, DbType.Int32);
        parameters.Add("pageSize", request.PageSize, DbType.Int32);

        string whereClause = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : string.Empty;

        string sortBy = request.SortBy?.ToLower() switch
        {
            "name" => "d.name",
            "path" => "d.path",
            "created" => "d.created_at",
            _ => "d.name",
        };

        string sortDirection = request.SortDirection?.ToLower() == "desc" ? "DESC" : "ASC";

        string orderByClause = $"ORDER BY {sortBy} {sortDirection}, d.id {sortDirection}";

        var connection = _dbConnectionFactory.GetDbConnection();

        long? totalCount = 0;

        var departments =
            await connection.QueryAsync<DepartmentShortDto, long, DepartmentShortDto>(
                $"""
                 SELECT d.id,
                        d.name,
                        d.parent_id,
                        d.is_active,
                        d.identifier,
                        d.path,
                        d.created_at,
                        d.updated_at,
                        d.deleted_at,
                        COUNT(*) OVER() AS total_count
                 FROM departments d
                 {whereClause}
                 {orderByClause}
                 LIMIT @pageSize OFFSET @page
                 """,
                splitOn: "total_count",
                map: (department, count) =>
                {
                    totalCount = count;
                    return department;
                }, param: parameters);

        return new PaginationEnvelope<DepartmentShortDto>(departments, totalCount ?? 0, request.Page, request.PageSize);
    }
}

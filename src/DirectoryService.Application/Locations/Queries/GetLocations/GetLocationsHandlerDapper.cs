using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Locations.GetLocations.Dtos;
using DirectoryService.Contracts.Locations.GetLocations.Requests;
using FluentValidation;
using Shared;
using Shared.Abstractions;
using Shared.Database;
using Shared.Response;
using Shared.Validation;

namespace DirectoryService.Application.Locations.Queries.GetLocations;

public class GetLocationsHandlerDapper : IQueryHandler<PaginationEnvelope<GetLocationsDto>, GetLocationsQuery>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IValidator<GetLocationsRequest> _validator;

    public GetLocationsHandlerDapper(
        IDbConnectionFactory dbConnectionFactory, IValidator<GetLocationsRequest> validator)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _validator = validator;
    }

    public async Task<Result<PaginationEnvelope<GetLocationsDto>, Errors>> Handle(
        GetLocationsQuery query, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(query.Request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var parameters = new DynamicParameters();
        var conditions = new List<string>();

        if (query.Request.DepartmentIds is { Length: > 0 } deptIds)
        {
            parameters.Add("departmentIds", deptIds);
            conditions.Add(
                """
                EXISTS(
                    SELECT 1
                    FROM department_locations dl
                    WHERE dl.location_id = l.id
                        AND dl.department_id = ANY (@departmentIds)
                )
                """);
        }

        if (!string.IsNullOrWhiteSpace(query.Request.Search))
        {
            parameters.Add("search", query.Request.Search, DbType.String);
            conditions.Add("l.name ILIKE '%' || @search || '%'");
        }

        if (query.Request.IsActive.HasValue)
        {
            parameters.Add("is_active", query.Request.IsActive.Value, DbType.Boolean);
            conditions.Add("l.is_active = @is_active");
        }

        parameters.Add("page", (query.Request.Page - 1) * query.Request.PageSize, DbType.Int32);
        parameters.Add("page_size", query.Request.PageSize, DbType.Int32);

        string whereClause = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : string.Empty;

        string sortBy = query.Request.SortBy?.ToLower() switch
        {
            "name" => "l.name",
            "created" => "l.created_at",
            _ => "l.name"
        };

        string sortDirection = query.Request.SortDirection?.ToLower() == "asc"
            ? "ASC"
            : "DESC";

        string orderByClause = $"ORDER BY {sortBy} {sortDirection}";

        var connection = _dbConnectionFactory.GetDbConnection();

        long? totalCount = 0;
        var locations = await connection.QueryAsync<GetLocationsDto, long, GetLocationsDto>(
            $"""
             SELECT 
                 l.id,
                 l.name,
                 l.is_active,
                 l.timezone,
                 l.address,
                 l.created_at,
                 l.updated_at,
                 COUNT(*) OVER () as total_count
             FROM locations l
             {whereClause}
             {orderByClause}
             LIMIT @page_size OFFSET @page
             """,
            splitOn: "total_count",
            map: (location, count) =>
            {
                totalCount = count;

                return location;
            }, param: parameters);

        return new PaginationEnvelope<GetLocationsDto>(locations.ToList(), totalCount ?? 0);
    }
}
using System.Data;
using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Locations.GetLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Shared;
using Shared.Abstractions;

namespace DirectoryService.Application.Locations.Queries.GetLocations;

public class GetLocationsHandler : IQueryHandler<PaginationResponse, GetLocationsQuery>
{
    private readonly IReadDbContext _readDbContext;
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IValidator<GetLocationsRequest> _validator;

    public GetLocationsHandler(
        IReadDbContext readDbContext, IDbConnectionFactory dbConnectionFactory,
        IValidator<GetLocationsRequest> validator)
    {
        _readDbContext = readDbContext;
        _dbConnectionFactory = dbConnectionFactory;
        _validator = validator;
    }

    public async Task<Result<PaginationResponse, Errors>> HandleDapper(
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

        parameters.Add("page", (query.Request.Pagination.Page - 1) * query.Request.Pagination.PageSize, DbType.Int32);
        parameters.Add("page_size", query.Request.Pagination.PageSize, DbType.Int32);

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

        return new PaginationResponse(locations.ToList(), totalCount ?? 0);
    }

    public async Task<Result<PaginationResponse, Errors>> Handle(
        GetLocationsQuery query, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(query.Request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var locationQuery = _readDbContext.LocationsRead;

        if (query.Request.DepartmentIds is { Length: > 0 } deptIds)
        {
            var deptIdsVo = deptIds.Select(DepartmentId.Create).ToArray();

            locationQuery = locationQuery.Where(
                l =>
                    l.Departments.Any(dl => deptIdsVo.Contains(dl.DepartmentId)));
        }

        if (!string.IsNullOrWhiteSpace(query.Request.Search))
        {
            locationQuery = locationQuery.Where(
                l => EF.Functions.Like(
                    l.Name.Value.ToLower(),
                    $"%{query.Request.Search.ToLower()}%"));
        }

        if (query.Request.IsActive.HasValue)
        {
            locationQuery = locationQuery.Where(l => l.IsActive == query.Request.IsActive.Value);
        }

        Expression<Func<Location, object>> sortBy = query.Request.SortBy?.ToLower() switch
        {
            "name" => l => l.Name.Value,
            "created" => l => l.CreatedAt,
            _ => l => l.Name.Value
        };

        locationQuery = query.Request.SortDirection?.ToLower() == "asc"
            ? locationQuery.OrderBy(sortBy)
            : locationQuery.OrderByDescending(sortBy);

        locationQuery = locationQuery
            .Skip((query.Request.Pagination.Page - 1) * query.Request.Pagination.PageSize)
            .Take(query.Request.Pagination.PageSize);

        int totalCount = await locationQuery.CountAsync(cancellationToken);

        var locations = await locationQuery.Select(
                l => new GetLocationsDto()
                {
                    Id = l.Id.Value,
                    Name = l.Name.Value,
                    Timezone = l.Timezone.Value,
                    Address = new AddressDto(l.Address.Country, l.Address.City, l.Address.Street, l.Address.House),
                    UpdatedAt = l.UpdatedAt,
                    CreatedAt = l.CreatedAt,
                    IsActive = l.IsActive,
                })
            .ToListAsync(cancellationToken);

        return new PaginationResponse(locations.ToList(), totalCount);
    }
}
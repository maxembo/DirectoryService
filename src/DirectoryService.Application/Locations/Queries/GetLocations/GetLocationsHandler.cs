using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Locations.GetLocations.Dtos;
using DirectoryService.Contracts.Locations.GetLocations.Requests;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Shared;
using Shared.Abstractions;
using Shared.Response;
using Shared.Validation;

namespace DirectoryService.Application.Locations.Queries.GetLocations;

public class GetLocationsHandler : IQueryHandler<PaginationEnvelope<GetLocationsDto>, GetLocationsQuery>
{
    private readonly IReadDbContext _readDbContext;
    private readonly IValidator<GetLocationsRequest> _validator;

    public GetLocationsHandler(IReadDbContext readDbContext, IValidator<GetLocationsRequest> validator)
    {
        _readDbContext = readDbContext;
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

        return new PaginationEnvelope<GetLocationsDto>(locations.ToList(), totalCount);
    }
}
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Locations.GetLocations.Dtos;
using DirectoryService.Contracts.Locations.GetLocations.Requests;
using DirectoryService.Domain.Departments;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SharedService.Core.Abstractions;
using SharedService.Core.Validation;
using SharedService.SharedKernel;
using SharedService.SharedKernel.Response;

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

            locationQuery = locationQuery.Where(l =>
                l.Departments.Any(dl => deptIdsVo.Contains(dl.DepartmentId)));
        }

        if (!string.IsNullOrWhiteSpace(query.Request.Search))
        {
            locationQuery = locationQuery.Where(l => EF.Functions.Like(
                l.Name.Value,
                $"%{query.Request.Search.Trim()}%"));
        }

        if (query.Request.IsActive.HasValue)
        {
            locationQuery = locationQuery.Where(l => l.IsActive == query.Request.IsActive.Value);
        }

        int totalCount = await locationQuery.CountAsync(cancellationToken);

        bool isAsc = query.Request.SortDirection?.ToLower() == "asc";
        locationQuery = query.Request.SortBy?.ToLower() switch
        {
            "created" => isAsc
                ? locationQuery.OrderBy(l => l.CreatedAt).ThenBy(l => l.Id.Value)
                : locationQuery.OrderByDescending(l => l.CreatedAt).ThenByDescending(l => l.Id.Value),
            "name" or _ => isAsc
                ? locationQuery.OrderBy(l => l.Name.Value).ThenBy(l => l.Id.Value)
                : locationQuery.OrderByDescending(l => l.Name.Value).ThenByDescending(l => l.Id.Value),
        };

        locationQuery = locationQuery
            .Skip((query.Request.Page - 1) * query.Request.PageSize)
            .Take(query.Request.PageSize);

        var locations = await locationQuery.Select(l => new GetLocationsDto()
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

        return new PaginationEnvelope<GetLocationsDto>(
            locations.ToList(), totalCount, query.Request.Page, query.Request.PageSize);
    }
}
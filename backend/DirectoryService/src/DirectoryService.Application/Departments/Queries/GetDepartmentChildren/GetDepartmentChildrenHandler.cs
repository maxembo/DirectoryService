using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Constants;
using DirectoryService.Contracts.Departments.GetDepartments.Dtos;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using SharedService.Core.Abstractions;
using SharedService.Core.Database;
using SharedService.Core.Validation;
using SharedService.SharedKernel;
using SharedService.SharedKernel.Response;

namespace DirectoryService.Application.Departments.Queries.GetDepartmentChildren;

public class GetDepartmentChildrenHandler
    : IQueryHandler<PaginationEnvelope<GetDepartmentChildrenDto>, GetDepartmentChildrenQuery>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IValidator<GetDepartmentChildrenQuery> _validator;
    private readonly HybridCache _cache;

    public GetDepartmentChildrenHandler(
        IDbConnectionFactory dbConnectionFactory,
        IValidator<GetDepartmentChildrenQuery> validator,
        HybridCache cache)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _validator = validator;
        _cache = cache;
    }

    public async Task<Result<PaginationEnvelope<GetDepartmentChildrenDto>, Errors>> Handle(
        GetDepartmentChildrenQuery query, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        return await GetPresignedDepartmentChildrenFromCache(query, cancellationToken);
    }

    private async Task<PaginationEnvelope<GetDepartmentChildrenDto>> GetPresignedDepartmentChildrenFromCache(
        GetDepartmentChildrenQuery query, CancellationToken cancellationToken)
    {
        string key = CacheKeys.CreateDepartmentsKey(
            "parentId", query.ParentId.ToString(),
            "page", query.Request.Page.ToString(),
            "pageSize", query.Request.PageSize.ToString());

        return await _cache.GetOrCreateAsync(
            key,
            factory: async _ =>
            {
                var result = await GetDepartmentChildren(query);

                return result.IsFailure
                    ? new PaginationEnvelope<GetDepartmentChildrenDto>([], 0, 0, 0)
                    : result.Value;
            },
            tags: [CacheKeys.DEPARTMENT_KEY],
            cancellationToken: cancellationToken);
    }

    private async Task<Result<PaginationEnvelope<GetDepartmentChildrenDto>>> GetDepartmentChildren(
        GetDepartmentChildrenQuery query)
    {
        const string sql = """
                           SELECT d.id,
                                  d.parent_id,
                                  d.name,
                                  d.identifier,
                                  d.depth,
                                  d.is_active,
                                  d.created_at,
                                  d.updated_at,
                           
                                  (EXISTS(SELECT 1
                                          FROM departments
                                          WHERE parent_id = d.id)) AS has_more_children,
                           
                                  COUNT(*) OVER ()                 AS total_count
                           FROM departments d
                           WHERE d.parent_id = @ParentId
                             AND d.is_active
                           ORDER BY d.created_at
                           LIMIT @ChildSize OFFSET @ChildPage
                           """;

        var dbConnection = _dbConnectionFactory.GetDbConnection();

        long? totalCount = null;

        var childrenDepartments = (await dbConnection.QueryAsync<GetDepartmentChildrenDto, bool, long, GetDepartmentChildrenDto>(
            sql,
            splitOn: "has_more_children, total_count",
            map: (dto, children, c) =>
            {
                totalCount ??= c;

                dto = dto with { HasChildren = children };

                return dto;
            },
            param:
            new
            {
                ParentId = query.ParentId,
                ChildSize = query.Request.PageSize,
                ChildPage = (query.Request.Page - 1) * query.Request.PageSize,
            })).ToList();

        return new PaginationEnvelope<GetDepartmentChildrenDto>(
            childrenDepartments, totalCount ?? 0, query.Request.Page, query.Request.PageSize);
    }
}
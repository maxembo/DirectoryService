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

namespace DirectoryService.Application.Departments.Queries.GetChildrenDepartments;

public class GetChildrenDepartmentsHandler
    : IQueryHandler<PaginationEnvelope<GetDepartmentDto>, GetChildrenDepartmentsQuery>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IValidator<GetChildrenDepartmentsQuery> _validator;
    private readonly HybridCache _cache;

    public GetChildrenDepartmentsHandler(
        IDbConnectionFactory dbConnectionFactory,
        IValidator<GetChildrenDepartmentsQuery> validator,
        HybridCache cache)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _validator = validator;
        _cache = cache;
    }

    public async Task<Result<PaginationEnvelope<GetDepartmentDto>, Errors>> Handle(
        GetChildrenDepartmentsQuery query, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        return await GetPresignedChildrenDepartmentsFromCache(query, cancellationToken);
    }

    private async Task<PaginationEnvelope<GetDepartmentDto>> GetPresignedChildrenDepartmentsFromCache(
        GetChildrenDepartmentsQuery query, CancellationToken cancellationToken)
    {
        string key = CacheKeys.CreateDepartmentsKey(
            "parentId", query.ParentId.ToString(),
            "page", query.Request.Page.ToString(),
            "pageSize", query.Request.PageSize.ToString());

        return await _cache.GetOrCreateAsync(
            key,
            factory: async _ =>
            {
                var result = await GetChildrenDepartments(query);

                return result.IsFailure
                    ? new PaginationEnvelope<GetDepartmentDto>([], 0, 0, 0)
                    : result.Value;
            },
            tags: [CacheKeys.DEPARTMENT_KEY],
            cancellationToken: cancellationToken);
    }

    private async Task<Result<PaginationEnvelope<GetDepartmentDto>>> GetChildrenDepartments(
        GetChildrenDepartmentsQuery query)
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

        var childrenDepartments = (await dbConnection.QueryAsync<GetDepartmentDto, bool, long, GetDepartmentDto>(
            sql,
            splitOn: "has_more_children, total_count",
            map: (dto, children, c) =>
            {
                totalCount ??= c;

                dto = dto with { HasMoreChildren = children };

                return dto;
            },
            param:
            new
            {
                ParentId = query.ParentId,
                ChildSize = query.Request.PageSize,
                ChildPage = (query.Request.Page - 1) * query.Request.PageSize,
            })).ToList();

        return new PaginationEnvelope<GetDepartmentDto>(
            childrenDepartments, totalCount ?? 0, query.Request.Page, query.Request.PageSize);
    }
}
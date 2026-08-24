using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Constants;
using DirectoryService.Contracts.Departments.GetDepartments.Dtos;
using DirectoryService.Contracts.Departments.GetDepartments.Requests;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using SharedService.Core.Abstractions;
using SharedService.Core.Database;
using SharedService.Core.Validation;
using SharedService.SharedKernel;
using SharedService.SharedKernel.Response;

namespace DirectoryService.Application.Departments.Queries.GetDepartmentTreeRoots;

public class
    GetDepartmentTreeRootsHandler : IQueryHandler<PaginationEnvelope<GetDepartmentTreeRootsDto>,
    GetDepartmentTreeRootsQuery>
{
    private readonly HybridCache _cache;
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IValidator<GetDepartmentTreeRootsRequest> _validator;

    public GetDepartmentTreeRootsHandler(
        IDbConnectionFactory dbConnectionFactory,
        IValidator<GetDepartmentTreeRootsRequest> validator,
        HybridCache cache)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _validator = validator;
        _cache = cache;
    }

    public async Task<Result<PaginationEnvelope<GetDepartmentTreeRootsDto>, Errors>> Handle(
        GetDepartmentTreeRootsQuery query, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(query.Request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        return await GetPresignedDepartmentTreeRootsFromCache(query, cancellationToken);
    }

    private async Task<PaginationEnvelope<GetDepartmentTreeRootsDto>> GetPresignedDepartmentTreeRootsFromCache(
        GetDepartmentTreeRootsQuery query, CancellationToken cancellationToken)
    {
        string key = CacheKeys.CreateDepartmentsKey(
            "prefetch", query.Request.Prefetch.ToString(),
            "onlyActive", query.Request.OnlyActive.ToString(),
            "page", query.Request.Page.ToString(),
            "pageSize", query.Request.PageSize.ToString());

        return await _cache.GetOrCreateAsync(
            key,
            async _ =>
            {
                var result =
                    await GetDepartmentTreeRoots(query);
                return result.IsFailure
                    ? new PaginationEnvelope<GetDepartmentTreeRootsDto>([], 0, 0, 0)
                    : result.Value;
            },
            tags: [CacheKeys.DEPARTMENT_KEY],
            cancellationToken: cancellationToken);
    }

    private async Task<Result<PaginationEnvelope<GetDepartmentTreeRootsDto>, Errors>> GetDepartmentTreeRoots(
        GetDepartmentTreeRootsQuery query)
    {
        const string sql = """
                      WITH roots AS (SELECT d.id,
                                            d.parent_id,
                                            d.name,
                                            d.identifier,
                                            d.depth,
                                            d.path,
                                            d.is_active,
                                            d.created_at,
                                            d.updated_at,
                                            COUNT(*) OVER () AS total_count
                                     FROM departments d
                                     WHERE d.parent_id IS NULL
                                       AND d.deleted_at IS NULL
                                       AND (@OnlyActive = FALSE OR d.is_active = TRUE)
                                     ORDER BY d.created_at
                                     LIMIT @RootSize OFFSET @RootPage),

                           ranked_children AS (SELECT d.id,
                                                      d.parent_id,
                                                      d.name,
                                                      d.identifier,
                                                      d.depth,
                                                      d.path,
                                                      d.is_active,
                                                      d.created_at,
                                                      d.updated_at,
                                                      ROW_NUMBER() OVER (PARTITION BY d.parent_id ORDER BY d.created_at) AS child_rank,
                                                      COUNT(*) OVER ()                                                   AS total_count
                                               FROM departments d
                                                        JOIN roots r ON d.parent_id = r.id
                                               WHERE d.deleted_at IS NULL
                                                 AND (@OnlyActive = FALSE OR d.is_active = TRUE))

                      SELECT r.id,
                             r.parent_id,
                             r.name,
                             r.identifier,
                             r.depth,
                             r.path,
                             r.is_active,
                             r.created_at,
                             r.updated_at,
                             r.total_count,

                             (EXISTS(SELECT 1
                                     FROM departments d
                                     WHERE d.parent_id = r.id
                                       AND d.deleted_at IS NULL
                                       AND (@OnlyActive = FALSE OR d.is_active = TRUE))) AS has_more_children
                      FROM roots r

                      UNION ALL

                      SELECT rc.id,
                             rc.parent_id,
                             rc.name,
                             rc.identifier,
                             rc.depth,
                             rc.path,
                             rc.is_active,
                             rc.created_at,
                             rc.updated_at,
                             rc.total_count,

                             (EXISTS(SELECT 1
                                     FROM departments d
                                     WHERE d.parent_id = rc.id
                                       AND d.deleted_at IS NULL
                                       AND (@OnlyActive = FALSE OR d.is_active = TRUE))) AS has_more_children

                      FROM ranked_children rc
                      WHERE rc.child_rank <= @Prefetch;
                      """;

        var dbConnection = _dbConnectionFactory.GetDbConnection();

        long? totalCount = null;

        var departments =
            (await dbConnection.QueryAsync<GetDepartmentTreeRootsDto, long, bool, GetDepartmentTreeRootsDto>(
                sql,
                splitOn: "total_count, has_more_children",
                map: (dto, c, children) =>
                {
                    totalCount ??= c;

                    dto = dto with { HasChildren = children };

                    return dto;
                },
                param: new
                {
                    query.Request.OnlyActive,
                    RootSize = query.Request.PageSize,
                    RootPage = (query.Request.Page - 1) * query.Request.PageSize,
                    Prefetch = query.Request.Prefetch,
                })).ToList();

        var departmentDictionary = departments.ToDictionary(d => d.Id);
        List<GetDepartmentTreeRootsDto> roots = [];

        foreach (var department in departmentDictionary.Values)
        {
            if (department.ParentId.HasValue &&
                departmentDictionary.TryGetValue(department.ParentId.Value, out var parent))
            {
                parent.Children.Add(department.Id);
            }
            else
            {
                roots.Add(department);
            }
        }

        return new PaginationEnvelope<GetDepartmentTreeRootsDto>(
            roots, totalCount ?? 0, query.Request.Page, query.Request.PageSize);
    }
}

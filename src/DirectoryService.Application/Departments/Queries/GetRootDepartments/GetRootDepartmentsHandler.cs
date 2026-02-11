using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Contracts.Departments.GetDepartments.Dtos;
using DirectoryService.Contracts.Departments.GetDepartments.Requests;
using FluentValidation;
using Shared;
using Shared.Abstractions;
using Shared.Database;
using Shared.Response;
using Shared.Validation;

namespace DirectoryService.Application.Departments.Queries.GetRootDepartments;

public class
    GetRootDepartmentsHandler : IQueryHandler<PaginationEnvelope<GetDepartmentDto>, GetRootDepartmentsQuery>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IValidator<GetRootDepartmentsRequest> _validator;

    public GetRootDepartmentsHandler(
        IDbConnectionFactory dbConnectionFactory, IValidator<GetRootDepartmentsRequest> validator)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _validator = validator;
    }

    public async Task<Result<PaginationEnvelope<GetDepartmentDto>, Errors>> Handle(
        GetRootDepartmentsQuery query, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(query.Request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

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
                                                    WHERE d.is_active = true)

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
                                          WHERE d.parent_id = r.id)) AS has_more_children
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
                                          WHERE d.parent_id = rc.id)) AS has_more_children

                           FROM ranked_children rc
                           WHERE rc.child_rank <= @Prefetch
                           """;

        var dbConnection = _dbConnectionFactory.GetDbConnection();

        long? totalCount = null;

        var departments = (await dbConnection.QueryAsync<GetDepartmentDto, long, bool, GetDepartmentDto>(
            sql,
            splitOn: "total_count, has_more_children",
            map: (dto, c, children) =>
            {
                totalCount ??= c;

                dto = dto with { HasMoreChildren = children };

                return dto;
            },
            param: new
            {
                RootSize = query.Request.Pagination.PageSize,
                RootPage = (query.Request.Pagination.Page - 1) * query.Request.Pagination.PageSize,
                Prefetch = query.Request.Prefetch,
            })).ToList();

        var departmentDictionary = departments.ToDictionary(d => d.Id);
        List<GetDepartmentDto> roots = [];

        foreach (var department in departmentDictionary.Values)
        {
            if (department.ParentId.HasValue &&
                departmentDictionary.TryGetValue(department.ParentId.Value, out var parent))
            {
                parent.Childrens.Add(department.Id);
            }
            else
            {
                roots.Add(department);
            }
        }

        return new PaginationEnvelope<GetDepartmentDto>(roots, totalCount ?? 0);
    }
}
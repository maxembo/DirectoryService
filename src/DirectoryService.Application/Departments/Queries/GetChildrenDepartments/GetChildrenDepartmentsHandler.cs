using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Contracts.Departments.GetDepartments.Dtos;
using FluentValidation;
using Shared;
using Shared.Abstractions;
using Shared.Database;
using Shared.Response;
using Shared.Validation;

namespace DirectoryService.Application.Departments.Queries.GetChildrenDepartments;

public class
    GetChildrenDepartmentsHandler : IQueryHandler<PaginationEnvelope<GetDepartmentDto>, GetChildrenDepartmentsQuery>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IValidator<GetChildrenDepartmentsQuery> _validator;

    public GetChildrenDepartmentsHandler(
        IDbConnectionFactory dbConnectionFactory, IValidator<GetChildrenDepartmentsQuery> validator)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _validator = validator;
    }

    public async Task<Result<PaginationEnvelope<GetDepartmentDto>, Errors>> Handle(
        GetChildrenDepartmentsQuery query, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

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
                ChildSize = query.Request.Pagination.PageSize,
                ChildPage = (query.Request.Pagination.Page - 1) * query.Request.Pagination.PageSize,
            })).ToList();

        return new PaginationEnvelope<GetDepartmentDto>(childrenDepartments, totalCount ?? 0);
    }
}
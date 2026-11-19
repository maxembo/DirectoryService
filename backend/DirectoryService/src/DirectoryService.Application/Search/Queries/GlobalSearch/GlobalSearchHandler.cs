using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Contracts.Search.GlobalSearch;
using FluentValidation;
using SharedService.Core.Abstractions;
using SharedService.Core.Database;
using SharedService.Core.Validation;
using SharedService.SharedKernel;
using SharedService.SharedKernel.Response;

namespace DirectoryService.Application.Search.Queries.GlobalSearch;

public class GlobalSearchHandler : IQueryHandler<PaginationEnvelope<GlobalSearchDto>, GlobalSearchQuery>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IValidator<GlobalSearchQuery> _validator;

    public GlobalSearchHandler(
        IDbConnectionFactory dbConnectionFactory,
        IValidator<GlobalSearchQuery> validator)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _validator = validator;
    }

    public async Task<Result<PaginationEnvelope<GlobalSearchDto>, Errors>> Handle(
        GlobalSearchQuery query, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var dbConnection = _dbConnectionFactory.GetDbConnection();

        const string sql = """
                           SELECT id,
                                  type,
                                  title,
                                  subtitle,
                                  is_active,
                                  deleted_at,
                                  ROW_NUMBER() OVER (
                                      PARTITION BY type
                                      ORDER BY is_active DESC, title, id
                                      ) AS group_rank
                           FROM ((SELECT id,
                                         'department' AS type,
                                         name         AS title,
                                         path::text   AS subtitle,
                                         is_active,
                                         deleted_at
                                  FROM departments d
                                  WHERE name ILIKE '%' || @q || '%')
                           
                                 UNION ALL
                           
                                 (SELECT id,
                                         'position'                    AS type,
                                         name                          AS title,
                                         (SELECT STRING_AGG(d.name, ', ')
                                          FROM departments d
                                                   JOIN department_positions dp ON dp.department_id = d.id
                                          WHERE dp.position_id = p.id) AS subtitle,
                                         is_active,
                                         deleted_at
                                  FROM positions p
                                  WHERE name ILIKE '%' || @q || '%')
                           
                                 UNION ALL
                           
                                 (SELECT id,
                                         'location'                                           AS type,
                                         name                                                 AS title,
                                         CONCAT_WS(', ', address ->> 'Country', address ->> 'City',
                                                   address ->> 'Street', address ->> 'House') AS subtitle,
                                         is_active,
                                         deleted_at
                                  FROM locations
                           
                                  WHERE name ILIKE '%' || @q || '%'
                                     OR address ->> 'City' ILIKE '%' || @q || '%'
                                     OR address ->> 'Country' ILIKE '%' || @q || '%'
                                     OR address ->> 'Street' ILIKE '%' || @q || '%'
                                     OR address ->> 'House' ILIKE '%' || @q || '%')) AS results
                           ORDER BY group_rank, type
                           LIMIT 15;
                           """;

        var result = await dbConnection.QueryAsync<GlobalSearchDto>(sql, new { q = query.Q });

        var globalSearchDtos = result.ToList();

        return new PaginationEnvelope<GlobalSearchDto>(globalSearchDtos.ToArray(), globalSearchDtos.Count(), 0, 0);
    }
}
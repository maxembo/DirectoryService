using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Infrastructure.Postgres.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedService.SharedKernel;
using Path = DirectoryService.Domain.Departments.Path;

namespace DirectoryService.Infrastructure.Postgres.Departments;

public class DepartmentsRepository : IDepartmentsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;
    private readonly ILogger<DepartmentsRepository> _logger;

    public DepartmentsRepository(DirectoryServiceDbContext dbContext, ILogger<DepartmentsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> AddAsync(
        Department department, CancellationToken cancellationToken = default)
    {
        await _dbContext.AddAsync(department, cancellationToken);

        var saveChangesResult = await _dbContext.SaveChangesResultAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
        {
            return saveChangesResult.Error;
        }

        return department.Id.Value;
    }

    public async Task<Result<Department, Error>> GetByIdAsync(
        DepartmentId departmentId, CancellationToken cancellationToken = default)
    {
        var department = await _dbContext.Departments
            .Where(d => d.IsActive == true)
            .FirstOrDefaultAsync(d => departmentId == d.Id, cancellationToken);

        if (department == null)
        {
            return GeneralErrors.NotFound("department", departmentId.Value);
        }

        return department;
    }

    public async Task<UnitResult<Errors>> CheckExistingAndActiveAsync(
        Guid[] ids, CancellationToken cancellationToken = default)
    {
        var departmentIds = DepartmentId.Create(ids);

        var existingIds = await _dbContext.Departments
            .Where(d => departmentIds.Contains(d.Id) && d.IsActive)
            .Select(d => d.Id.Value)
            .ToListAsync(cancellationToken);

        var missingIds = ids.Except(existingIds)
            .ToList();

        var errors = missingIds
            .Select(missingId => GeneralErrors.NotFound("department", missingId))
            .ToList();

        return errors.Count != 0
            ? UnitResult.Failure(new Errors(errors))
            : UnitResult.Success<Errors>();
    }

    public async Task<UnitResult<Error>> DeleteLocationsAsync(
        DepartmentId departmentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.DepartmentLocations
                .Where(location => location.DepartmentId == departmentId)
                .ExecuteDeleteAsync(cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to delete location links for department {DepartmentId}",
                departmentId.Value);

            return GeneralErrors.Database(
                "department.locations.delete.failed",
                "Не удалось удалить связи подразделения с локациями.");
        }
    }

    public async Task<Result<Department, Error>> GetActiveByIdWithLock(
        DepartmentId id, CancellationToken cancellationToken = default)
    {
        var department = await _dbContext.Departments
            .FromSql($"SELECT d.* FROM departments d WHERE d.id = {id.Value} AND d.is_active = TRUE FOR UPDATE")
            .FirstOrDefaultAsync(cancellationToken);

        if (department is null)
        {
            return GeneralErrors.NotFound("departmentId", id.Value);
        }

        return department;
    }

    public async Task<Result<Department, Error>> GetByIdWithLock(
        DepartmentId id, CancellationToken cancellationToken = default)
    {
        var department = await _dbContext.Departments
            .FromSql($"SELECT d.* FROM departments d WHERE d.id = {id.Value} FOR UPDATE")
            .FirstOrDefaultAsync(cancellationToken);

        if (department is null)
        {
            return GeneralErrors.NotFound("departmentId", id.Value);
        }

        return department;
    }

    public async Task LockDescendants(Path path, CancellationToken cancellationToken = default)
    {
        const string sql = """
                           SELECT * 
                           FROM departments
                           WHERE path <@ @parentPath::ltree
                           ORDER BY depth
                           FOR UPDATE
                           """;

        var dbConnection = _dbContext.Database.GetDbConnection();

        var sqlParams = new { parentPath = path.Value };

        await dbConnection.QueryAsync(sql, sqlParams);
    }

    public async Task<bool> HasActiveDescendants(Path path, CancellationToken cancellationToken = default)
    {
        const string sql = """
                           SELECT EXISTS(
                               SELECT 1
                               FROM departments
                               WHERE path <@ @parentPath::ltree
                                 AND path <> @parentPath::ltree
                                 AND deleted_at IS NULL
                                 AND is_active = TRUE
                                 AND NOT path ~ '*.delete-*.*'::lquery)
                           """;

        var command = new CommandDefinition(
            sql,
            new { parentPath = path.Value },
            cancellationToken: cancellationToken);

        return await _dbContext.Database.GetDbConnection().QuerySingleAsync<bool>(command);
    }

    public async Task<UnitResult<Error>> MoveDepartments(
        Path parentPath,
        Path departmentPath,
        CancellationToken cancellationToken = default)
    {
        const string sqlUpdatePathAndDepth = """
                                             UPDATE departments
                                             SET path = @parentPath::ltree || subpath(path, nlevel(@departmentPath::ltree) - 1),
                                             depth = nlevel(@parentPath::ltree || subpath(path, nlevel(@departmentPath::ltree) - 1)) - 1
                                             WHERE path <@ @departmentPath::ltree
                                             """;

        var dbConnection = _dbContext.Database.GetDbConnection();

        try
        {
            var sqlUpdatePathAndDepthParams = new
            {
                parentPath = parentPath.Value,
                departmentPath = departmentPath.Value,
            };

            await dbConnection.ExecuteAsync(sqlUpdatePathAndDepth, sqlUpdatePathAndDepthParams);

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to move Department");

            return GeneralErrors.Database(message: ex.Message);
        }
    }

    public async Task<UnitResult<Error>> MoveDepartments(
        Path departmentPath,
        CancellationToken cancellationToken = default)
    {
        const string sqlUpdatePathAndDepth = """
                                             UPDATE departments
                                             SET path = subpath(path, nlevel(@departmentPath::ltree) - 1),
                                             depth = nlevel(subpath(path, nlevel(@departmentPath::ltree) - 1)) - 1
                                             WHERE path <@ @departmentPath::ltree
                                             """;

        var dbConnection = _dbContext.Database.GetDbConnection();

        try
        {
            await dbConnection.ExecuteAsync(sqlUpdatePathAndDepth, new { departmentPath = departmentPath.Value });

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to move Department");

            return GeneralErrors.Database(message: ex.Message);
        }
    }

    public async Task<UnitResult<Error>> CheckParentIsChild(
        Path parentPath, Path departmentPath, CancellationToken cancellationToken = default)
    {
        const string sql = """
                           SELECT id
                           FROM departments
                           WHERE path = @parentPath::ltree AND path <@ @departmentPath::ltree
                           ORDER BY depth
                           """;

        var dbConnection = _dbContext.Database.GetDbConnection();

        try
        {
            var sqlParams = new { parentPath = parentPath.Value, departmentPath = departmentPath.Value };

            var result = await dbConnection.QueryAsync(sql, sqlParams);

            if (result.Any())
            {
                return DepartmentErrors.MoveWouldCreateCycle();
            }

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to check Parent IsChild");

            return GeneralErrors.Database(message: ex.Message);
        }
    }

    public async Task<Result<Department, Error>> GetBy(
        Expression<Func<Department, bool>> predicate, CancellationToken cancellationToken)
    {
        var department = await _dbContext.Departments.FirstOrDefaultAsync(predicate, cancellationToken);

        if (department is null)
        {
            return GeneralErrors.NotFound("department");
        }

        return department;
    }

    public async Task<UnitResult<Error>> UpdatePathsMarkDelete(
        Path departmentPath, CancellationToken cancellationToken = default)
    {
        const string sql = """
                           UPDATE departments d
                           SET path = CASE 
                               WHEN d.depth > 0 
                                    THEN 
                                        (subpath(path, 0, nlevel(@departmentPath::ltree) - 1) 
                                            || ('delete-' 
                                            || subpath(path, nlevel(@departmentPath::ltree) - 1)::text))::ltree
                                    ELSE
                                        ('delete-' || subpath(path, nlevel(@departmentPath::ltree) - 1)::text)::ltree
                                END
                           WHERE path <@ @departmentPath::ltree
                           """;

        var dbConnection = _dbContext.Database.GetDbConnection();

        try
        {
            await dbConnection.ExecuteAsync(sql, new { departmentPath = departmentPath.Value });

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to update paths mark delete department");

            return GeneralErrors.Database(message: ex.Message);
        }
    }

    public async Task<UnitResult<Error>> DeleteDepartmentsMarkDelete(CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Departments
                .Where(d => d.IsActive == false && d.DeletedAt < DateTime.UtcNow.AddMonths(-1))
                .ExecuteDeleteAsync(cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to delete departments mark delete departments");

            return GeneralErrors.Database(message: ex.Message);
        }
    }

    public async Task<UnitResult<Error>> DeleteDepartmentLocationsMarkDelete(
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.DepartmentLocations
                .Where(dl => _dbContext.Departments
                    .Where(d => d.IsActive == false && d.DeletedAt < DateTime.UtcNow.AddMonths(-1))
                    .Select(d => d.Id)
                    .Contains(dl.DepartmentId))
                .ExecuteDeleteAsync(cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to delete department locations mark delete departments");

            return GeneralErrors.Database(message: ex.Message);
        }
    }

    public async Task<UnitResult<Error>> DeleteDepartmentPositionsMarkDelete(
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.DepartmentPositions
                .Where(dl => _dbContext.Departments
                    .Where(d => d.IsActive == false && d.DeletedAt < DateTime.UtcNow.AddMonths(-1))
                    .Select(d => d.Id)
                    .Contains(dl.DepartmentId))
                .ExecuteDeleteAsync(cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to delete department locations mark delete departments");

            return GeneralErrors.Database(message: ex.Message);
        }
    }

    public async Task<UnitResult<Error>> UpdatePathsAfterDelete(CancellationToken cancellationToken = default)
    {
        const string sql = """
                           WITH RECURSIVE expired AS MATERIALIZED (
                               SELECT id
                               FROM departments
                               WHERE is_active = false
                                 AND deleted_at < (NOW() - INTERVAL '1 month')
                           ),
                           rebuilt AS (
                               SELECT d.id,
                                      e.id IS NOT NULL AS is_expired,
                                      CASE WHEN e.id IS NULL THEN d.id END AS survivor_id,
                                      CASE
                                          WHEN e.id IS NULL
                                              THEN subpath(d.path, nlevel(d.path) - 1, 1)
                                      END AS survivor_path,
                                      NULL::uuid AS new_parent_id,
                                      CASE
                                          WHEN e.id IS NULL
                                              THEN subpath(d.path, nlevel(d.path) - 1, 1)
                                      END AS new_path,
                                      CASE WHEN e.id IS NULL THEN 0::smallint END AS new_depth
                               FROM departments d
                               LEFT JOIN expired e ON e.id = d.id
                               WHERE d.parent_id IS NULL

                               UNION ALL

                               SELECT child.id,
                                      e.id IS NOT NULL AS is_expired,
                                      CASE
                                          WHEN e.id IS NULL THEN child.id
                                          ELSE parent.survivor_id
                                      END AS survivor_id,
                                      CASE
                                          WHEN e.id IS NULL THEN paths.new_path
                                          ELSE parent.survivor_path
                                      END AS survivor_path,
                                      CASE
                                          WHEN e.id IS NULL THEN parent.survivor_id
                                      END AS new_parent_id,
                                      CASE
                                          WHEN e.id IS NULL THEN paths.new_path
                                      END AS new_path,
                                      CASE
                                          WHEN e.id IS NULL
                                              THEN (nlevel(paths.new_path) - 1)::smallint
                                      END AS new_depth
                               FROM departments child
                               JOIN rebuilt parent ON child.parent_id = parent.id
                               LEFT JOIN expired e ON e.id = child.id
                               CROSS JOIN LATERAL (
                                   SELECT CASE
                                              WHEN parent.survivor_path IS NULL
                                                  THEN subpath(child.path, nlevel(child.path) - 1, 1)
                                              ELSE parent.survivor_path
                                                   || subpath(child.path, nlevel(child.path) - 1, 1)
                                          END AS new_path
                               ) paths
                           )
                           UPDATE departments d
                           SET parent_id = r.new_parent_id,
                               path = r.new_path,
                               depth = r.new_depth
                           FROM rebuilt r
                           WHERE d.id = r.id
                             AND r.is_expired = false
                             AND (d.parent_id IS DISTINCT FROM r.new_parent_id
                                  OR d.path IS DISTINCT FROM r.new_path
                                  OR d.depth IS DISTINCT FROM r.new_depth);
                           """;

        var dbConnection = _dbContext.Database.GetDbConnection();

        try
        {
            await dbConnection.ExecuteAsync(sql);

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to update paths delete departments");

            return GeneralErrors.Database(message: ex.Message);
        }
    }

    public async Task<UnitResult<Error>> RestoreSubtreePaths(
        Path departmentPath,
        Path restoredPath,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           UPDATE departments d
                           SET path = CASE
                                          WHEN d.path = @departmentPath::ltree
                                              THEN
                                              @restoredPath::ltree
                                          ELSE (@restoredPath::ltree || subpath(d.path, nlevel(@departmentPath::ltree)))
                               END
                           WHERE path <@ @departmentPath::ltree;
                           """;

        var dbConnection = _dbContext.Database.GetDbConnection();

        var sqlParams = new { departmentPath = departmentPath.Value, restoredPath = restoredPath.Value };

        try
        {
            await dbConnection.ExecuteAsync(
                new CommandDefinition(sql, sqlParams, cancellationToken: cancellationToken));

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to restore paths for department subtree {DepartmentPath}",
                departmentPath.Value);

            return GeneralErrors.Database(message: ex.Message);
        }
    }
}

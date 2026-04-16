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
            return saveChangesResult.Error;

        return department.Id.Value;
    }

    public async Task<Result<Department, Error>> GetByIdAsync(
        DepartmentId departmentId, CancellationToken cancellationToken = default)
    {
        var department = await _dbContext.Departments
            .Where(d => d.IsActive == true)
            .FirstOrDefaultAsync(d => departmentId == d.Id, cancellationToken);

        if (department == null)
            return GeneralErrors.NotFound("department", departmentId.Value);

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
        DepartmentId departmentId, CancellationToken cancellationToken = default)
    {
        await _dbContext.DepartmentLocations
            .Where(dl => dl.DepartmentId == departmentId)
            .ExecuteDeleteAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }

    public async Task<Result<Department, Error>> GetByIdWithLock(
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

    public async Task<UnitResult<Error>> MoveDepartments(
        DepartmentId parentId,
        Path parentPath, Path departmentPath,
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
                parentPath = parentPath.Value, departmentPath = departmentPath.Value,
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
            await dbConnection.ExecuteAsync(sqlUpdatePathAndDepth, new { departmentPath = departmentPath.Value, });

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
                return GeneralErrors.Invalid("department.parentId");

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
            await dbConnection.ExecuteAsync(sql, param: new { departmentPath = departmentPath.Value });

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
                .Where(
                    dl => _dbContext.Departments
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
                .Where(
                    dl => _dbContext.Departments
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
                           WITH outdated_departments AS (SELECT *
                                                         FROM departments d
                                                         WHERE d.is_active = false
                                                           AND d.deleted_at < (NOW() - INTERVAL '1 month'))

                           UPDATE departments d
                           SET path      = CASE
                                               WHEN d.path = od.path
                                                   THEN subpath(d.path, 0, nlevel(od.path::ltree) - 1)
                                               ELSE subpath(d.path, 0, nlevel(od.path::ltree) - 1)
                                                   || subpath(d.path, nlevel(od.path::ltree))
                               END,
                               depth     = CASE
                                               WHEN d.path = od.path THEN 0
                                               ELSE d.depth - 1
                                   END,
                               parent_id = CASE
                                               WHEN d.path = od.path THEN NULL
                                               WHEN d.depth - 1 = 0 THEN NULL
                                               ELSE (SELECT id
                                                     FROM departments dp
                                                     WHERE dp.path = subpath(
                                                             CASE
                                                                 WHEN od.depth = 0
                                                                     THEN subpath(d.path, nlevel(od.path::ltree) - 1)
                                                                 ELSE subpath(d.path, 0, nlevel(od.path::ltree) - 1)
                                                                     || subpath(d.path, nlevel(od.path::ltree))
                                                                 END,
                                                             0,
                                                             -1))
                                   END
                           FROM outdated_departments od
                           WHERE d.path <@ od.path;
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
}
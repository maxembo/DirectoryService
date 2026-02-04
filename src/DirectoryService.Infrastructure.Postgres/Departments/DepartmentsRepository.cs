using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Infrastructure.Postgres.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared;
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
            return GeneralErrors.NotFound(departmentId.Value, "department");

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
            .Select(missingId => GeneralErrors.NotFound(missingId, "department"))
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
            return GeneralErrors.NotFound(id.Value, "departmentId");
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

            return GeneralErrors.Database(null, ex.Message);
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

            return GeneralErrors.Database(null, ex.Message);
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

            return GeneralErrors.Database(null, ex.Message);
        }
    }
}
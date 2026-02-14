using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Positions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using DirectoryService.Infrastructure.Postgres.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Infrastructure.Postgres.Positions;

public class PositionsRepository : IPositionsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;
    private readonly ILogger<PositionsRepository> _logger;

    public PositionsRepository(DirectoryServiceDbContext dbContext, ILogger<PositionsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> AddAsync(Position position, CancellationToken cancellationToken = default)
    {
        await _dbContext.AddAsync(position, cancellationToken);

        var saveChangesResult = await _dbContext.SaveChangesResultAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Error;

        return position.Id.Value;
    }

    public async Task<UnitResult<Error>> DeleteUnusedPositionsByDepartmentIdAsync(
        DepartmentId departmentId, CancellationToken cancellationToken = default)
    {
        const string sql = """
                           UPDATE positions p
                           SET is_active  = false,
                               deleted_at = NOW()
                           FROM department_positions dp
                           WHERE dp.position_id = p.id
                             AND dp.department_id = @departmentId
                             AND NOT EXISTS (SELECT 1
                                             FROM department_positions dp2
                                                      JOIN departments d ON dp.department_id = d.id
                                             WHERE dp2.position_id = dp.position_id
                                               AND d.is_active = true
                                               AND d.id <> @departmentId)
                             AND p.is_active = true
                           """;

        var dbConnection = _dbContext.Database.GetDbConnection();

        try
        {
            await dbConnection.ExecuteAsync(sql, param: new { departmentId = departmentId.Value });

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to delete positions");

            return GeneralErrors.Database(null, ex.Message);
        }
    }
}
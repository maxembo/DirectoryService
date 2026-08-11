using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Positions;
using DirectoryService.Domain;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using DirectoryService.Infrastructure.Postgres.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedService.SharedKernel;

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

    public async Task<UnitResult<Error>> SoftDeleteUnusedPositionsByDepartmentIdAsync(
        DepartmentId departmentId, CancellationToken cancellationToken = default)
    {
        const string sql = """
                           UPDATE positions p
                           SET is_active  = false,
                               deleted_at = NOW(),
                               updated_at = NOW(),
                               deletion_reason = @deletionReason
                           FROM department_positions dp
                           WHERE dp.position_id = p.id
                             AND dp.department_id = @departmentId
                             AND NOT EXISTS (SELECT 1
                                             FROM department_positions dp2
                                                      JOIN departments d ON dp2.department_id = d.id
                                             WHERE dp2.position_id = dp.position_id
                                               AND d.is_active = true
                                               AND d.id <> @departmentId)
                             AND p.is_active = true
                           """;

        var dbConnection = _dbContext.Database.GetDbConnection();

        try
        {
            await dbConnection.ExecuteAsync(
                sql,
                param: new
                {
                    departmentId = departmentId.Value,
                    deletionReason = DeletionReason.NO_ACTIVE_DEPARTMENTS.ToString(),
                });

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to soft delete positions for department {DepartmentId}",
                departmentId.Value);

            return GeneralErrors.Database(message: "Не удалось пометить как мягкое удаление позиции подразделения.");
        }
    }

    public async Task<UnitResult<Error>> RestorePositionsByDepartmentIdAsync(
        DepartmentId departmentId, CancellationToken cancellationToken = default)
    {
        const string sql = """
                           UPDATE positions p
                           SET is_active  = true,
                               deleted_at = NULL,
                               updated_at = NOW(),
                               deletion_reason = NULL
                           FROM department_positions dp
                           WHERE dp.position_id = p.id
                             AND dp.department_id = @departmentId
                             AND p.is_active = false
                             AND p.deletion_reason = @deletionReason
                           """;

        var dbConnection = _dbContext.Database.GetDbConnection();

        try
        {
            await dbConnection.ExecuteAsync(
                sql,
                param: new
                {
                    departmentId = departmentId.Value,
                    deletionReason = DeletionReason.NO_ACTIVE_DEPARTMENTS.ToString(),
                });

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to restore positions for department {DepartmentId}",
                departmentId.Value);

            return GeneralErrors.Database(message: "Не удалось восстановить позиции подразделения.");
        }
    }

    public async Task<UnitResult<Error>> DeletePositionsMarkDelete(CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Positions
                .Where(p => p.IsActive == false && p.DeletedAt < DateTime.UtcNow.AddMonths(-1))
                .ExecuteDeleteAsync(cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to delete positions");

            return GeneralErrors.Database(ex.Message);
        }
    }

    public async Task<Result<Position, Error>> GetByIdAsync(
        PositionId positionId, CancellationToken cancellationToken = default)
    {
        var position = await _dbContext.Positions
            .Where(p => p.IsActive)
            .FirstOrDefaultAsync(p => p.Id == positionId, cancellationToken);

        if (position == null)
        {
            return GeneralErrors.NotFound("position", positionId.Value);
        }

        return position;
    }
}
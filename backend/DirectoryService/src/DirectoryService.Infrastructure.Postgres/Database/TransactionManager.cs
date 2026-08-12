using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using SharedService.Core.Database;
using SharedService.SharedKernel;

namespace DirectoryService.Infrastructure.Postgres.Database;

public class TransactionManager : ITransactionManager
{
    private readonly DirectoryServiceDbContext _dbContext;
    private readonly ILogger<TransactionManager> _logger;
    private readonly ILoggerFactory _loggerFactory;

    public TransactionManager(
        DirectoryServiceDbContext dbContext,
        ILoggerFactory loggerFactory,
        ILogger<TransactionManager> logger)
    {
        _dbContext = dbContext;
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    public async Task<Result<ITransactionScope, Error>> BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            IDbContextTransaction? transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            ILogger<TransactionScope>? transactionCreateLogger = _loggerFactory.CreateLogger<TransactionScope>();

            var transactionScope = new TransactionScope(transaction.GetDbTransaction(), transactionCreateLogger);

            return transactionScope;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return GeneralErrors.Database("database", "Failed to begin transaction.");
        }
    }

    public async Task<UnitResult<Error>> SaveChangeAsync(CancellationToken cancellationToken = default)
    {
        UnitResult<Error> saveChangesResultAsync = await _dbContext.SaveChangesResultAsync(cancellationToken);
        if (saveChangesResultAsync.IsFailure)
        {
            return saveChangesResultAsync.Error;
        }

        return UnitResult.Success<Error>();
    }
}
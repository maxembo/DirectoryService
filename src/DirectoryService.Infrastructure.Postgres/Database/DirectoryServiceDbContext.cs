using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared;

namespace DirectoryService.Infrastructure.Postgres.Database;

public class DirectoryServiceDbContext(IConfiguration configuration) : DbContext
{
    private const string DATABASE = "DirectoryServiceDb";

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<Location> Locations => Set<Location>();

    public DbSet<DepartmentLocation> DepartmentLocations => Set<DepartmentLocation>();

    public async Task<UnitResult<Error>> SaveChangesResultAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                string constraintName = pgEx.ConstraintName!;

                string name = constraintName.Split('_').Last();

                return GeneralErrors.AlreadyExist(name);
            }

            return GeneralErrors.Database(ex.Message);
        }
        catch (Exception ex)
        {
            return GeneralErrors.Database(ex.Message);
        }

        return UnitResult.Success<Error>();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(configuration.GetConnectionString(DATABASE));
        optionsBuilder.UseLoggerFactory(CreateLoggerFactory());
        optionsBuilder.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(DirectoryServiceDbContext).Assembly);

    private ILoggerFactory CreateLoggerFactory()
        => LoggerFactory.Create(configure => configure.AddConsole());
}
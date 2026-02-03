using System.Data.Common;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared;

namespace DirectoryService.Infrastructure.Postgres.Database;

public class DirectoryServiceDbContext(string connectionString) : DbContext, IReadDbContext, IDbConnectionFactory
{
    public DbSet<Department> Departments => Set<Department>();

    public DbSet<Location> Locations => Set<Location>();

    public DbSet<DepartmentLocation> DepartmentLocations => Set<DepartmentLocation>();

    public IQueryable<Department> DepartmentsRead => Set<Department>().AsQueryable().AsNoTracking();

    public IQueryable<Location> LocationsRead => Set<Location>().AsQueryable().AsNoTracking();

    public IQueryable<DepartmentLocation> DepartmentLocationsRead =>
        Set<DepartmentLocation>().AsQueryable().AsNoTracking();

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
        optionsBuilder.UseNpgsql(connectionString);
        optionsBuilder.UseLoggerFactory(CreateLoggerFactory());
        optionsBuilder.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("ltree");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DirectoryServiceDbContext).Assembly);
    }

    private ILoggerFactory CreateLoggerFactory()
        => LoggerFactory.Create(configure => configure.AddConsole());

    public DbConnection GetDbConnection() => Database.GetDbConnection();
}
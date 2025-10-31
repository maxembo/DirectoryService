using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Postgres;

public class DirectoryServiceDbContext(IConfiguration configuration) : DbContext
{
    private const string DATABASE = "DirectoryServiceDb";

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
using System.Globalization;
using DirectoryService.Web;
using Serilog;
using SharedService.Framework.Middlewares;
using Environments = SharedService.Framework.Environments;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();
try
{
    Log.Information("Starting DirectoryService.Web");

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddProgramDependencies(builder.Configuration);

    var app = builder.Build();

    app.Configure();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

namespace DirectoryService.Web
{
    public partial class Program;
}
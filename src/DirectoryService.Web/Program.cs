using System.Globalization;
using DirectoryService.Presentation.Response;
using DirectoryService.Web;
using DirectoryService.Web.Middlewares;
using Microsoft.OpenApi.Models;
using Serilog;
using Shared;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();
try
{
    Log.Information("Starting DirectoryService.Web");

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddProgramDependencies(builder.Configuration);

    builder.Services.AddOpenApi(
        options =>
        {
            options.AddSchemaTransformer(
                (schema, context, _) =>
                {
                    if (context.JsonTypeInfo.Type == typeof(Envelope<Errors>))
                    {
                        if (schema.Properties.TryGetValue("errors", out var errorsProp))
                        {
                            errorsProp.Items.Reference = new OpenApiReference()
                            {
                                Type = ReferenceType.Schema, Id = "Error",
                            };
                        }
                    }

                    return Task.CompletedTask;
                });
        });

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "DirectoryService"));
    }

    app.UseExceptionMiddleware();

    app.MapControllers();

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
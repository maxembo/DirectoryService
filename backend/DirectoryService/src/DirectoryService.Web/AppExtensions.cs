using Serilog;
using SharedService.Framework.Middlewares;
using Environments = SharedService.Framework.Environments;

namespace DirectoryService.Web;

public static class AppExtensions
{
    public static IApplicationBuilder Configure(this WebApplication app)
    {
        string[] allowedOrigins = app.Configuration["CORS_ALLOWED_ORIGINS"]?
                                      .Split(
                                          ',',
                                          StringSplitOptions.RemoveEmptyEntries |
                                          StringSplitOptions.TrimEntries)
                                  ?? [];

        app.UseCors(
            builder =>
            {
                builder.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });

        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment(Environments.DOCKER))
        {
            app.MapOpenApi();
            app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "DirectoryService"));
        }

        app.UseExceptionMiddleware();

        app.MapControllers();

        return app;
    }
}
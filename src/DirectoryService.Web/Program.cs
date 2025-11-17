using DirectoryService.Presentation.Response;
using DirectoryService.Web;
using Microsoft.OpenApi.Models;
using Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProgramDependencies();

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
                            Type = ReferenceType.Schema,
                            Id = "Error",
                        };
                    }
                }

                return Task.CompletedTask;
            });
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "DirectoryService"));
}

app.MapControllers();

app.Run();
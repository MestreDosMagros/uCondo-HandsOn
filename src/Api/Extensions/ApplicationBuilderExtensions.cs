using Infrastructure;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Api.Extensions;

public static class ApplicationBuilderExtensions
{
    private static JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static IApplicationBuilder ApplyMigrations(this IApplicationBuilder builder)
    {
        using (var scope = builder.ApplicationServices.CreateScope())
        {
            var services = scope.ServiceProvider;

            try
            {
                // Somente por que o MySql demora a subir usando o compose
                Task.Delay(TimeSpan.FromSeconds(45)).GetAwaiter().GetResult();

                var context = services.GetRequiredService<Context>();
                context.Database.Migrate();
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while migrating the database.");
                throw;
            }
        }

        return builder;
    }

    public static void UseProblemDetailsExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(builder =>
        {
            builder.Run(async context =>
            {
                var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();

                if (exceptionHandlerFeature != null)
                {
                    var exception = exceptionHandlerFeature.Error;

                    var problemDetails = new ProblemDetails
                    {
                        Instance = context.Request.HttpContext.Request.Path
                    };

                    if (exception is BadHttpRequestException badHttpRequestException)
                    {
                        problemDetails.Title = "The request is invalid";
                        problemDetails.Status = StatusCodes.Status400BadRequest;
                        problemDetails.Detail = badHttpRequestException.Message;
                    }
                    else
                    {
                        problemDetails.Title = exception.Message;
                        problemDetails.Status = StatusCodes.Status500InternalServerError;
                        problemDetails.Detail = exception.ToString();
                    }

                    context.Response.StatusCode = problemDetails.Status.Value;
                    context.Response.ContentType = "application/problem+json";

                    await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, _jsonSerializerOptions));
                }
            });
        });
    }
}

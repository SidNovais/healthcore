using System.Reflection;
using Microsoft.OpenApi.Models;

namespace HC.LIS.API.Configuration.Extensions;

internal static class SwaggerExtensions
{
    internal static IServiceCollection AddSwaggerDocumentation(
        this IServiceCollection services,
        string title,
        string description)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = title,
                Version = "v1",
                Description = description
            });

            // Full type names prevent schema name conflicts between modules
            options.CustomSchemaIds(t => t.ToString());

            // Include XML doc comments generated from /// summaries
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var commentsFile = Path.Combine(
                baseDirectory,
                $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");

            if (File.Exists(commentsFile))
                options.IncludeXmlComments(commentsFile);

            // JWT Bearer security definition
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            // Apply Bearer requirement globally
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    internal static IApplicationBuilder UseSwaggerDocumentation(
        this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "HC.LIS API v1");
            c.RoutePrefix = "swagger";
        });

        return app;
    }
}

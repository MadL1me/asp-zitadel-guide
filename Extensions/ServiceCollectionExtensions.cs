using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Zitadel.Extensions;
using ZitadelExample.Authentication;
using ZitadelExample.Configuration;

namespace ZitadelExample.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddZitadelAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var authConfig = configuration.GetSection(nameof(ZitadelConfiguration)).Get<ZitadelConfiguration>();

        if (authConfig is null)
        {
            throw new ApplicationException("Auth configuration cannot be null: " +
                                           "Setup ZitadelConfiguration section with required data");
        }

        services.AddSingleton<IAuthorizationHandler, ZitadelRoleHandler>();

        services
            .AddAuthorization(options =>
            {
                options.AddPolicy(AuthConstants.PolicyTest, policy =>
                    policy.Requirements.Add(new HasZitadelRoleRequirement("test")));
            })
            .AddAuthentication(o =>
            {
            })
            .AddZitadelIntrospection(
                AuthConstants.Schema,
                o =>
                {
                    o.Authority = authConfig.ZitadelHostUrl;
                    o.ClientId = authConfig.ClientId;
                    o.ClientSecret = authConfig.ClientSecret;
                    o.EnableCaching = false;
                    o.Validate();
                });

        return services;
    }

    public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Zitadel Example", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "Access Token (PAN or JWT)"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

        services.AddEndpointsApiExplorer();
        return services;
    }
}

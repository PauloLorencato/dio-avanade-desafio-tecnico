using Microsoft.OpenApi.Models;
using NLog;
using NLog.Layouts;
using LogLevel = NLog.LogLevel;

namespace Estoque;

public static class Startup
{
    public static void ConfigureServices(IServiceCollection services, ConfigurationManager configManager)
    {
        AddSwaggerOptions(services);
        services.AddHealthChecks();

    }

    public static void AddSwaggerOptions(IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Formato: Bearer {token}"
            });

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
            new string[] { }
        }
            });
        });
    }

    public static void AddMetrics(IServiceCollection services)
    {
        services.AddMetrics();
    }

    public static void AddNlogConfigs(IServiceCollection services)
    {
        var config = new NLog.Config.LoggingConfiguration();
        // Adiciona filtro para ignorar logs de HttpClient e Microsoft.AspNetCore
        var conditionBasedFilter = new NLog.Filters.ConditionBasedFilter
        {
            Condition = $"contains('${{logger}}', 'System.Net.Http.HttpClient') or contains('${{logger}}', 'Microsoft.AspNetCore')",
            Action = NLog.Filters.FilterResult.IgnoreFinal
        };

        var rule = new NLog.Config.LoggingRule("*", NLog.LogLevel.Trace, new NLog.Targets.NullTarget());
        rule.Filters.Add(conditionBasedFilter);
        config.LoggingRules.Add(rule);

        var logstdout = new NLog.Targets.ConsoleTarget("stdout");
        var jsonLayout = new JsonLayout
        {
            Attributes =
    {
        new JsonAttribute("level", "${level:upperCase=true}"),
        new JsonAttribute("traceId", "${event-properties:item=traceId}"),
        new JsonAttribute("datetimestart", "${event-properties:item=datetimestart}"),
        new JsonAttribute("datetimeend", "${event-properties:item=datetimeend}"),
        new JsonAttribute("service", "${event-properties:item=service}"),
        new JsonAttribute("message", "${message}"),
        new JsonAttribute("exception", "${exception:format=tostring}")
    }
        };
        logstdout.Layout = jsonLayout;

        config.AddRule(LogLevel.Info, LogLevel.Fatal, logstdout);

        // Apply config           
        LogManager.Configuration = config;

        // Add services to the container.
        services.AddTransient<Logger>(sp => LogManager.GetLogger("stdout"));
    }
}
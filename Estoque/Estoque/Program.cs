using Estoque;
using NLog.Web;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    // Adiciona suporte a variáveis de ambiente
    .AddEnvironmentVariables()
    .Build();

// Define a porta da aplicação. Priioridade: 1 - Variável de ambiente, 2 - appsettings.json, 3 - Padrão 80
var apiPort = builder.Configuration["Ports:Api"] ?? "80";
builder.WebHost.UseUrls($"http://0.0.0.0:{apiPort}");

Startup.ConfigureServices(builder.Services, builder.Configuration);
builder.Logging.ClearProviders();
builder.Host.UseNLog();


builder.Services.AddControllers();

var app = builder.Build();

// Configura healthchecks
app.MapHealthChecks("/health");

app.UseSwagger();
app.UseSwaggerUI();

var metricsPort = builder.Configuration["Ports:Metrics"] ?? "8081";
using var metricServer = new KestrelMetricServer(port: int.Parse(metricsPort), url: "/metrics", hostname: "0.0.0.0");
metricServer.Start();
app.UseHttpMetrics();

app.UseHttpsRedirection();

System.Console.WriteLine("Build and run successful");
System.Console.WriteLine($"Running api on port: {apiPort}");
System.Console.WriteLine($"Exposing metrics on port: {metricsPort}");
app.Run();
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Portfolio.Api.Extensions;
using Portfolio.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();
builder.Services.AddApiFoundation(builder.Configuration);

var app = builder.Build();
app.UseExceptionHandler();
app.UseMiddleware<CorrelationIdMiddleware>();
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseConfiguredCors();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = registration => !registration.Tags.Contains("ready")
});
app.MapHealthChecks("/health/ready");
app.MapGet("/api", () => Results.Ok(new { name = "Portfolio.Api", status = "ready" }))
    .WithName("ApiVerification");
app.Run();

public partial class Program;

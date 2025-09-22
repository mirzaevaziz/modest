using System.Globalization;
using FastEndpoints;
using FastEndpoints.Swagger;
using FluentValidation;
using FluentValidation.AspNetCore;
using Modest.API.Handlers;
using Modest.Core;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .WriteTo.File(
        "logs/app-.log",
        rollingInterval: RollingInterval.Day,
        formatProvider: CultureInfo.InvariantCulture
    )
    .CreateLogger();

builder.Host.UseSerilog();

// Register validators from the current assembly
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.SwaggerDocument();

builder
    .Services.AddFastEndpoints()
    .AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.AddCoreServices();

// Register the API exception handler
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Use the new IExceptionHandler-based handler
app.UseExceptionHandler();

// Use FastEndpoints and Swagger
app.UseFastEndpoints();
app.UseSwaggerGen();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Log all HTTP requests
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

app.Run();

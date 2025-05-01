using Microsoft.Extensions.Options;
using LoggingBroker.WebApi.Extensions.Services;
using LoggingBroker.Application;
using LoggingBroker.WebApi.Extensions.Endpoints;
using LoggingBroker.WebApi.Extensions.Exceptions;
using LoggingBroker.Infrastructure;
using LoggingBroker.Infrastructure.Services.Loki;

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilogLogging();
builder.Services.AddInfrastructureServices();

builder.Services.AddApplicationServices();

builder.Services.AddBrokerServices(builder.Configuration);

var app = builder.Build();

var validator = app.Services.GetRequiredService<IStartupValidator>();
validator.Validate();

app.UseCustomExceptionHandler();
app.UseRouting();
app.UseRateLimiter();

app.MapRoutes();

app.Run();


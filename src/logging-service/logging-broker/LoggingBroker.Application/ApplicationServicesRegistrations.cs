using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Reflection;
using LoggingBroker.Application.Services.Validations;

namespace LoggingBroker.Application;

public static class ApplicationServicesRegistrations
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssembly(Assembly.Load("LoggingBroker.Application"));

        services.AddScoped<IValidationDispatcher, ValidationDispatcher>();

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

        });

        return services;
    }
}

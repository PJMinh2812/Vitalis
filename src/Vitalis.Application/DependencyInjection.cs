using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Vitalis.Application.Interfaces;
using Vitalis.Application.Services;

namespace Vitalis.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IPatientService, PatientService>();

        return services;
    }
}

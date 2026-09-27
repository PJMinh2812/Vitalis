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
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISpecialtyService, SpecialtyService>();
        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<IDoctorScheduleService, DoctorScheduleService>();
        services.AddScoped<IDoctorTimeOffService, DoctorTimeOffService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IMedicalRecordsService, MedicalRecordsService>();
        services.AddScoped<IMedicineService, MedicineService>();
        services.AddScoped<IServiceCatalogService, ServiceCatalogService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IPharmacyService, PharmacyService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}

using Vitalis.Domain.Entities.Scheduling;

namespace Vitalis.Application.DTOs.Patients;

// No AutoMapper in this project (deliberate) — every module maps its entity to
// its DTO explicitly like this, so copy this pattern for the next modules too.
public static class PatientMappingExtensions
{
    public static PatientDto ToDto(this Patient patient) => new(
        patient.Id,
        patient.PatientCode,
        patient.FullName,
        patient.Gender,
        patient.DateOfBirth,
        patient.Phone,
        patient.Email,
        patient.Address,
        patient.NationalId,
        patient.InsuranceNumber,
        patient.BloodType,
        patient.EmergencyContactName,
        patient.EmergencyContactPhone,
        patient.UserId is not null,
        patient.IsActive,
        patient.CreatedAt);
}

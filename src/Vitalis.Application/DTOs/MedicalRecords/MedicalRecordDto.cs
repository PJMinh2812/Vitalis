using Vitalis.Domain.Enums;

namespace Vitalis.Application.DTOs.MedicalRecords;

public record MedicalRecordDto(
    int Id,
    int AppointmentId,
    int PatientId,
    string PatientName,
    int DoctorId,
    string DoctorName,
    string? Symptoms,
    string? Diagnosis,
    string? Icd10Code,
    string? TreatmentPlan,
    DateOnly? FollowUpDate,
    string? Note,
    MedicalRecordStatus Status,
    DateTime? FinalizedAt,
    VitalsDto? Vitals,
    List<MedicalRecordServiceDto> Services,
    PrescriptionDto? Prescription,
    DateTime CreatedAt);

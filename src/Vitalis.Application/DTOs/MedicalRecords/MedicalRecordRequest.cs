namespace Vitalis.Application.DTOs.MedicalRecords;

// VC-13 "Lưu nháp" — only allowed while the record is still Draft.
public record MedicalRecordRequest(
    string Symptoms,
    string Diagnosis,
    string? Icd10Code,
    string? TreatmentPlan,
    DateOnly? FollowUpDate,
    VitalsDto? Vitals);

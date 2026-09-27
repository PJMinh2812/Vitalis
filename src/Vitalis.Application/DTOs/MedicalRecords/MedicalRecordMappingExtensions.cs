using Vitalis.Domain.Entities.Clinical;

namespace Vitalis.Application.DTOs.MedicalRecords;

public static class MedicalRecordMappingExtensions
{
    public static VitalsDto ToDto(this PatientVitals vitals) => new(
        vitals.Temperature, vitals.Pulse, vitals.BloodPressure, vitals.Weight, vitals.Height);

    public static LabResultDto ToDto(this LabResult result) => new(
        result.Id, result.ResultValue, result.ReferenceRange, result.Conclusion, result.ResultedAt);

    public static MedicalRecordServiceDto ToDto(this MedicalRecordService mrs, string serviceName, LabResultDto? result) => new(
        mrs.Id, mrs.MedicalRecordId, mrs.ServiceId, serviceName, mrs.Quantity, mrs.UnitPriceSnapshot, mrs.Status, result);

    public static PrescriptionItemDto ToDto(this PrescriptionItem item) => new(
        item.Id, item.MedicineId, item.MedicineNameSnapshot, item.UnitPriceSnapshot, item.Quantity, item.Dosage, item.DurationDays, item.Instruction);

    public static PrescriptionDto ToDto(this Prescription prescription, List<PrescriptionItemDto> items) => new(
        prescription.Id, prescription.MedicalRecordId, prescription.DoctorId, prescription.Status, prescription.Note, items);

    public static MedicalRecordDto ToDto(
        this MedicalRecord record,
        string patientName,
        string doctorName,
        VitalsDto? vitals,
        List<MedicalRecordServiceDto> services,
        PrescriptionDto? prescription) => new(
        record.Id,
        record.AppointmentId,
        record.PatientId,
        patientName,
        record.DoctorId,
        doctorName,
        record.Symptoms,
        record.Diagnosis,
        record.Icd10Code,
        record.TreatmentPlan,
        record.FollowUpDate,
        record.Note,
        record.Status,
        record.FinalizedAt,
        vitals,
        services,
        prescription,
        record.CreatedAt);
}

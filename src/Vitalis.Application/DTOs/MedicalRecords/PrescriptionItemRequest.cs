namespace Vitalis.Application.DTOs.MedicalRecords;

// AllergyConfirmed must be true if the medicine's active ingredient matches one
// of the patient's recorded allergies — checked server-side, not trusted blindly.
public record PrescriptionItemRequest(
    int MedicineId,
    int Quantity,
    string Dosage,
    string? Instruction,
    int? DurationDays,
    bool AllergyConfirmed);

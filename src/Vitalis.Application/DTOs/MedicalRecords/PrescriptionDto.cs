using Vitalis.Domain.Enums;

namespace Vitalis.Application.DTOs.MedicalRecords;

public record PrescriptionDto(
    int Id,
    int MedicalRecordId,
    int DoctorId,
    PrescriptionStatus Status,
    string? Note,
    List<PrescriptionItemDto> Items);

using Vitalis.Domain.Enums;

namespace Vitalis.Application.DTOs.MedicalRecords;

public record MedicalRecordServiceDto(
    int Id,
    int MedicalRecordId,
    int ServiceId,
    string ServiceName,
    int Quantity,
    decimal UnitPriceSnapshot,
    MedicalRecordServiceStatus Status,
    LabResultDto? Result);

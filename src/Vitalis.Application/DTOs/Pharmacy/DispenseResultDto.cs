using Vitalis.Domain.Enums;

namespace Vitalis.Application.DTOs.Pharmacy;

public record DispenseBatchAllocationDto(
    int PrescriptionItemId,
    int MedicineId,
    int BatchId,
    string BatchNo,
    int Quantity);

public record DispenseResultDto(
    int PrescriptionId,
    PrescriptionStatus Status,
    DateTime? DispensedAt,
    List<DispenseBatchAllocationDto> Allocations);

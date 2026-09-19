namespace Vitalis.Application.DTOs.MedicalRecords;

public record PrescriptionRequest(string? Note, List<PrescriptionItemRequest> Items);

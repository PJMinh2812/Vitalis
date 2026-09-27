using Vitalis.Application.DTOs.Medicines;

namespace Vitalis.Application.Interfaces;

public interface IMedicineService
{
    Task<MedicineDto> CreateAsync(CreateMedicineRequest request, CancellationToken cancellationToken = default);

    // VC-15 #1: available stock + allergy warning against the given patient's history.
    Task<IReadOnlyList<MedicineSearchDto>> SearchAsync(string? keyword, int? patientId, CancellationToken cancellationToken = default);
}

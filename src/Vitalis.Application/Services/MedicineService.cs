using Vitalis.Application.DTOs.Medicines;
using Vitalis.Application.Exceptions;
using Vitalis.Application.Interfaces;
using Vitalis.Domain.Entities.Clinical;

namespace Vitalis.Application.Services;

public class MedicineService(IUnitOfWork unitOfWork) : IMedicineService
{
    public async Task<MedicineDto> CreateAsync(CreateMedicineRequest request, CancellationToken cancellationToken = default)
    {
        var repository = unitOfWork.Repository<Medicine>();

        if (request.Code is not null)
        {
            var codeTaken = await repository.FirstOrDefaultAsync(
                repository.Query().Where(m => m.Code == request.Code), cancellationToken) is not null;
            if (codeTaken)
                throw new ConflictException($"Mã thuốc '{request.Code}' đã tồn tại");
        }

        var medicine = new Medicine
        {
            Code = request.Code,
            Name = request.Name,
            ActiveIngredient = request.ActiveIngredient,
            Concentration = request.Concentration,
            Unit = request.Unit,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
        };
        await repository.AddAsync(medicine, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return medicine.ToDto();
    }

    public async Task<IReadOnlyList<MedicineSearchDto>> SearchAsync(string? keyword, int? patientId, CancellationToken cancellationToken = default)
    {
        var repository = unitOfWork.Repository<Medicine>();
        // VC-15: "Ẩn hoặc chặn thuốc tồn = 0" — hidden here rather than shown-disabled,
        // simplest reading of "ẩn".
        var query = repository.Query().Where(m => m.IsActive && m.StockQuantity > 0);

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(m => m.Name.StartsWith(keyword) || (m.ActiveIngredient != null && m.ActiveIngredient.StartsWith(keyword)));

        query = query.OrderBy(m => m.Name);
        var medicines = await repository.ToListAsync(query, cancellationToken);

        var allergenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (patientId is not null)
        {
            var allergyRepository = unitOfWork.Repository<PatientAllergy>();
            var allergies = await allergyRepository.ToListAsync(
                allergyRepository.Query().Where(a => a.PatientId == patientId), cancellationToken);
            allergenNames = allergies.Select(a => a.Allergen).ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        return medicines
            .Select(m => m.ToSearchDto(m.ActiveIngredient is not null && allergenNames.Contains(m.ActiveIngredient)))
            .ToList();
    }
}

using Vitalis.Domain.Entities.Clinical;

namespace Vitalis.Application.DTOs.Medicines;

public static class MedicineMappingExtensions
{
    public static MedicineDto ToDto(this Medicine medicine) => new(
        medicine.Id, medicine.Code, medicine.Name, medicine.ActiveIngredient, medicine.Concentration,
        medicine.Unit, medicine.Price, medicine.StockQuantity, medicine.IsActive);

    public static MedicineSearchDto ToSearchDto(this Medicine medicine, bool allergyWarning) => new(
        medicine.Id, medicine.Name, medicine.ActiveIngredient, medicine.Concentration,
        medicine.Unit, medicine.Price, medicine.StockQuantity, allergyWarning);
}

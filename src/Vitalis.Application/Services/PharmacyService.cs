using Vitalis.Application.DTOs.Medicines;
using Vitalis.Application.DTOs.Pharmacy;
using Vitalis.Application.Exceptions;
using Vitalis.Application.Interfaces;
using Vitalis.Domain.Entities.Clinical;
using Vitalis.Domain.Enums;
using ValidationException = Vitalis.Application.Exceptions.ValidationException;

namespace Vitalis.Application.Services;

public class PharmacyService(IUnitOfWork unitOfWork) : IPharmacyService
{
    public async Task<IReadOnlyList<MedicineInventoryDto>> GetInventoryAsync(bool? lowStock, bool? nearExpiry, CancellationToken cancellationToken = default)
    {
        var medicineRepository = unitOfWork.Repository<Medicine>();
        var medicines = await medicineRepository.ToListAsync(medicineRepository.Query().OrderBy(m => m.Name), cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var nearExpiryThreshold = today.AddDays(30);

        var batchRepository = unitOfWork.Repository<MedicineBatch>();
        var liveBatches = await batchRepository.ToListAsync(
            batchRepository.Query().Where(b => b.ExpiryDate >= today), cancellationToken);
        var batchesByMedicine = liveBatches.GroupBy(b => b.MedicineId).ToDictionary(g => g.Key, g => g.ToList());

        var result = new List<MedicineInventoryDto>();
        foreach (var medicine in medicines)
        {
            var batches = batchesByMedicine.GetValueOrDefault(medicine.Id, []);
            var totalStock = batches.Sum(b => b.Quantity);
            var nearestExpiry = batches.OrderBy(b => b.ExpiryDate).FirstOrDefault()?.ExpiryDate;
            var isLowStock = medicine.MinStock is not null && totalStock < medicine.MinStock;
            var isNearExpiry = nearestExpiry is not null && nearestExpiry <= nearExpiryThreshold;

            if (lowStock == true && !isLowStock) continue;
            if (nearExpiry == true && !isNearExpiry) continue;

            result.Add(new MedicineInventoryDto(
                medicine.Id, medicine.Code, medicine.Name, medicine.ActiveIngredient,
                totalStock, medicine.MinStock, isLowStock, nearestExpiry, isNearExpiry, medicine.IsActive));
        }

        return result;
    }

    public async Task<IReadOnlyList<MedicineDto>> CreateReceiptAsync(MedicineReceiptRequest request, int receivedBy, CancellationToken cancellationToken = default)
    {
        var medicineRepository = unitOfWork.Repository<Medicine>();
        var batchRepository = unitOfWork.Repository<MedicineBatch>();
        var transactionRepository = unitOfWork.Repository<MedicineStockTransaction>();

        var updatedMedicines = new List<Medicine>();

        foreach (var item in request.Items)
        {
            var medicine = await medicineRepository.GetByIdAsync(item.MedicineId, cancellationToken)
                ?? throw new NotFoundException(nameof(Medicine), item.MedicineId);

            // (medicine_id, batch_no) is UNIQUE — same lot number restocked adds
            // to the existing batch row instead of creating a duplicate.
            var batch = await batchRepository.FirstOrDefaultAsync(
                batchRepository.Query().Where(b => b.MedicineId == item.MedicineId && b.BatchNo == item.BatchNo), cancellationToken);

            if (batch is null)
            {
                batch = new MedicineBatch
                {
                    MedicineId = item.MedicineId,
                    BatchNo = item.BatchNo,
                    ExpiryDate = item.ExpiryDate,
                    Quantity = item.Quantity,
                    ImportPrice = item.ImportPrice,
                };
                await batchRepository.AddAsync(batch, cancellationToken);
            }
            else
            {
                batch.Quantity += item.Quantity;
                batch.ImportPrice = item.ImportPrice;
                batchRepository.Update(batch);
            }

            medicine.StockQuantity += item.Quantity;
            medicineRepository.Update(medicine);
            updatedMedicines.Add(medicine);

            await unitOfWork.SaveChangesAsync(cancellationToken); // need batch.Id for the transaction FK below

            await transactionRepository.AddAsync(new MedicineStockTransaction
            {
                MedicineId = item.MedicineId,
                BatchId = batch.Id,
                Type = StockTransactionType.In,
                Quantity = item.Quantity,
                CreatedBy = receivedBy,
            }, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return updatedMedicines.Select(m => m.ToDto()).ToList();
    }

    public async Task<DispenseResultDto> DispenseAsync(int prescriptionId, int dispensedBy, CancellationToken cancellationToken = default)
    {
        var prescriptionRepository = unitOfWork.Repository<Prescription>();
        var prescription = await prescriptionRepository.GetByIdAsync(prescriptionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Prescription), prescriptionId);

        if (prescription.Status != PrescriptionStatus.Pending)
            throw new ConflictException("Đơn thuốc đã được phát hoặc không hợp lệ");

        var itemRepository = unitOfWork.Repository<PrescriptionItem>();
        var items = await itemRepository.ToListAsync(itemRepository.Query().Where(i => i.PrescriptionId == prescriptionId), cancellationToken);

        var batchRepository = unitOfWork.Repository<MedicineBatch>();
        var transactionRepository = unitOfWork.Repository<MedicineStockTransaction>();
        var medicineRepository = unitOfWork.Repository<Medicine>();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var allocations = new List<DispenseBatchAllocationDto>();

        // Nothing is committed until the final SaveChangesAsync below — if any
        // line can't be fully covered, the exception aborts before that call and
        // every in-memory batch/medicine mutation up to that point is discarded
        // with the DbContext, so this is atomic without an explicit transaction.
        foreach (var item in items)
        {
            var candidateBatches = await batchRepository.ToListAsync(
                batchRepository.Query()
                    .Where(b => b.MedicineId == item.MedicineId && b.ExpiryDate >= today && b.Quantity > 0)
                    .OrderBy(b => b.ExpiryDate), // FEFO
                cancellationToken);

            var remaining = item.Quantity;
            foreach (var batch in candidateBatches)
            {
                if (remaining <= 0) break;

                var take = Math.Min(remaining, batch.Quantity);
                batch.Quantity -= take;
                batchRepository.Update(batch);

                await transactionRepository.AddAsync(new MedicineStockTransaction
                {
                    MedicineId = item.MedicineId,
                    BatchId = batch.Id,
                    Type = StockTransactionType.Out,
                    Quantity = take,
                    PrescriptionId = prescriptionId,
                    CreatedBy = dispensedBy,
                }, cancellationToken);

                allocations.Add(new DispenseBatchAllocationDto(item.Id, item.MedicineId, batch.Id, batch.BatchNo, take));
                remaining -= take;
            }

            if (remaining > 0)
                throw new ConflictException($"Không đủ tồn kho cho thuốc '{item.MedicineNameSnapshot}' (thiếu {remaining})");

            var medicine = await medicineRepository.GetByIdAsync(item.MedicineId, cancellationToken);
            if (medicine is not null)
            {
                medicine.StockQuantity -= item.Quantity;
                medicineRepository.Update(medicine);
            }
        }

        prescription.Status = PrescriptionStatus.Dispensed;
        prescription.DispensedAt = DateTime.UtcNow;
        prescription.DispensedBy = dispensedBy;
        prescriptionRepository.Update(prescription);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new DispenseResultDto(prescriptionId, prescription.Status, prescription.DispensedAt, allocations);
    }

    public async Task AdjustStockAsync(int medicineId, AdjustStockRequest request, int adjustedBy, CancellationToken cancellationToken = default)
    {
        var batchRepository = unitOfWork.Repository<MedicineBatch>();
        var batch = await batchRepository.GetByIdAsync(request.BatchId, cancellationToken)
            ?? throw new NotFoundException(nameof(MedicineBatch), request.BatchId);

        if (batch.MedicineId != medicineId)
            throw new ValidationException("batchId", "Lô không thuộc thuốc này");

        if (request.Quantity > batch.Quantity)
            throw new ConflictException("Số lượng điều chỉnh vượt quá tồn kho hiện tại của lô");

        batch.Quantity -= request.Quantity;
        batchRepository.Update(batch);

        var medicineRepository = unitOfWork.Repository<Medicine>();
        var medicine = await medicineRepository.GetByIdAsync(medicineId, cancellationToken)
            ?? throw new NotFoundException(nameof(Medicine), medicineId);
        medicine.StockQuantity -= request.Quantity;
        medicineRepository.Update(medicine);

        await unitOfWork.Repository<MedicineStockTransaction>().AddAsync(new MedicineStockTransaction
        {
            MedicineId = medicineId,
            BatchId = request.BatchId,
            Type = StockTransactionType.Adjust,
            Quantity = request.Quantity,
            CreatedBy = adjustedBy,
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StockTransactionDto>> GetTransactionsAsync(int medicineId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var repository = unitOfWork.Repository<MedicineStockTransaction>();
        var query = repository.Query().Where(t => t.MedicineId == medicineId);

        if (from is not null) query = query.Where(t => t.CreatedAt >= from);
        if (to is not null) query = query.Where(t => t.CreatedAt <= to);

        query = query.OrderByDescending(t => t.CreatedAt);
        var items = await repository.ToListAsync(query, cancellationToken);

        return items.Select(t => t.ToDto()).ToList();
    }
}

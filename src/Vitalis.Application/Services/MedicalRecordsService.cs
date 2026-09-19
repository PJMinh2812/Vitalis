using Vitalis.Application.Common;
using Vitalis.Application.DTOs.Billing;
using Vitalis.Application.DTOs.MedicalRecords;
using Vitalis.Application.Exceptions;
using Vitalis.Application.Interfaces;
using Vitalis.Domain.Entities.Billing;
using Vitalis.Domain.Entities.Clinical;
using Vitalis.Domain.Entities.Scheduling;
using Vitalis.Domain.Enums;
using ValidationException = Vitalis.Application.Exceptions.ValidationException;

namespace Vitalis.Application.Services;

// Owns the whole "visit" aggregate — MedicalRecord + its vitals/services/lab
// results/prescription — the same way PatientService owns Patient. These
// sub-resources have no independent admin screen of their own, so one service
// class for the aggregate is the right granularity (unlike Doctor/Specialty/
// Schedule/TimeOff, which each have their own admin screen).
public class MedicalRecordsService(IUnitOfWork unitOfWork) : IMedicalRecordsService
{
    public async Task<MedicalRecordDto> StartExamAsync(int appointmentId, CancellationToken cancellationToken = default)
    {
        var appointmentRepository = unitOfWork.Repository<Appointment>();
        var appointment = await appointmentRepository.GetByIdAsync(appointmentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Appointment), appointmentId);

        if (appointment.Status is not (AppointmentStatus.CheckedIn or AppointmentStatus.InProgress))
            throw new ConflictException("Chỉ có thể bắt đầu khám khi bệnh nhân đã check-in");

        var recordRepository = unitOfWork.Repository<MedicalRecord>();
        var existing = await recordRepository.FirstOrDefaultAsync(
            recordRepository.Query().Where(r => r.AppointmentId == appointmentId), cancellationToken);
        if (existing is not null)
            return await BuildDtoAsync(existing, cancellationToken);

        if (appointment.Status == AppointmentStatus.CheckedIn)
        {
            appointment.Status = AppointmentStatus.InProgress;
            appointment.UpdatedAt = DateTime.UtcNow;
            appointmentRepository.Update(appointment);
            await unitOfWork.Repository<AppointmentStatusHistory>().AddAsync(new AppointmentStatusHistory
            {
                AppointmentId = appointment.Id,
                FromStatus = AppointmentStatus.CheckedIn,
                ToStatus = AppointmentStatus.InProgress,
            }, cancellationToken);
        }

        var record = new MedicalRecord
        {
            AppointmentId = appointmentId,
            PatientId = appointment.PatientId,
            DoctorId = appointment.DoctorId,
            Status = MedicalRecordStatus.Draft,
        };
        await recordRepository.AddAsync(record, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await BuildDtoAsync(record, cancellationToken);
    }

    public async Task<MedicalRecordDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var record = await unitOfWork.Repository<MedicalRecord>().GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(MedicalRecord), id);

        return await BuildDtoAsync(record, cancellationToken);
    }

    public async Task<PagedResult<MedicalRecordDto>> GetPatientHistoryAsync(int patientId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var repository = unitOfWork.Repository<MedicalRecord>();
        var query = repository.Query()
            .Where(r => r.PatientId == patientId && r.Status == MedicalRecordStatus.Finalized)
            .OrderByDescending(r => r.FinalizedAt);

        var paged = await repository.GetPagedAsync(query, page, pageSize, cancellationToken);
        var items = new List<MedicalRecordDto>();
        foreach (var record in paged.Items)
            items.Add(await BuildDtoAsync(record, cancellationToken));

        return new PagedResult<MedicalRecordDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }

    public async Task<MedicalRecordDto> SaveDraftAsync(int id, MedicalRecordRequest request, CancellationToken cancellationToken = default)
    {
        var repository = unitOfWork.Repository<MedicalRecord>();
        var record = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(MedicalRecord), id);

        if (record.Status != MedicalRecordStatus.Draft)
            throw new ConflictException("Bệnh án đã chốt, không thể sửa");

        record.Symptoms = request.Symptoms;
        record.Diagnosis = request.Diagnosis;
        record.Icd10Code = request.Icd10Code;
        record.TreatmentPlan = request.TreatmentPlan;
        record.FollowUpDate = request.FollowUpDate;
        record.UpdatedAt = DateTime.UtcNow;
        repository.Update(record);

        if (request.Vitals is not null)
            await UpsertVitalsAsync(id, request.Vitals, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await BuildDtoAsync(record, cancellationToken);
    }

    public async Task<MedicalRecordServiceDto> OrderServiceAsync(int medicalRecordId, OrderServiceRequest request, CancellationToken cancellationToken = default)
    {
        var record = await unitOfWork.Repository<MedicalRecord>().GetByIdAsync(medicalRecordId, cancellationToken)
            ?? throw new NotFoundException(nameof(MedicalRecord), medicalRecordId);

        if (record.Status != MedicalRecordStatus.Draft)
            throw new ConflictException("Bệnh án đã chốt, không thể chỉ định thêm");

        var service = await unitOfWork.Repository<Service>().GetByIdAsync(request.ServiceId, cancellationToken)
            ?? throw new NotFoundException(nameof(Service), request.ServiceId);

        var mrs = new MedicalRecordService
        {
            MedicalRecordId = medicalRecordId,
            ServiceId = request.ServiceId,
            Quantity = request.Quantity,
            UnitPriceSnapshot = service.Price,
        };
        await unitOfWork.Repository<MedicalRecordService>().AddAsync(mrs, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mrs.ToDto(service.Name, null);
    }

    public async Task CancelServiceAsync(int medicalRecordServiceId, CancellationToken cancellationToken = default)
    {
        var repository = unitOfWork.Repository<MedicalRecordService>();
        var mrs = await repository.GetByIdAsync(medicalRecordServiceId, cancellationToken)
            ?? throw new NotFoundException(nameof(MedicalRecordService), medicalRecordServiceId);

        // "đã thực hiện thì vẫn phải tính phí" — only a still-Ordered (not yet
        // performed) line can be cancelled.
        if (mrs.Status != MedicalRecordServiceStatus.Ordered)
            throw new ConflictException("Chỉ định đã thực hiện hoặc đã huỷ, không thể huỷ");

        mrs.Status = MedicalRecordServiceStatus.Cancelled;
        repository.Update(mrs);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<MedicalRecordServiceDto> EnterResultAsync(int medicalRecordServiceId, LabResultRequest request, int performedBy, CancellationToken cancellationToken = default)
    {
        var repository = unitOfWork.Repository<MedicalRecordService>();
        var mrs = await repository.GetByIdAsync(medicalRecordServiceId, cancellationToken)
            ?? throw new NotFoundException(nameof(MedicalRecordService), medicalRecordServiceId);

        if (mrs.Status != MedicalRecordServiceStatus.Ordered)
            throw new ConflictException("Chỉ định đã hoàn tất hoặc đã huỷ");

        var labResultRepository = unitOfWork.Repository<LabResult>();
        var labResult = new LabResult
        {
            MedicalRecordServiceId = medicalRecordServiceId,
            ResultValue = request.ResultValue,
            ReferenceRange = request.ReferenceRange,
            Conclusion = request.Conclusion,
            ResultedAt = request.ResultedAt,
        };
        await labResultRepository.AddAsync(labResult, cancellationToken);

        mrs.Status = MedicalRecordServiceStatus.Completed;
        mrs.PerformedBy = performedBy;
        repository.Update(mrs);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var service = await unitOfWork.Repository<Service>().GetByIdAsync(mrs.ServiceId, cancellationToken);
        return mrs.ToDto(service?.Name ?? "", labResult.ToDto());
    }

    public async Task<PrescriptionDto> SavePrescriptionAsync(int medicalRecordId, PrescriptionRequest request, int doctorId, CancellationToken cancellationToken = default)
    {
        var record = await unitOfWork.Repository<MedicalRecord>().GetByIdAsync(medicalRecordId, cancellationToken)
            ?? throw new NotFoundException(nameof(MedicalRecord), medicalRecordId);

        if (record.Status != MedicalRecordStatus.Draft)
            throw new ConflictException("Bệnh án đã chốt, không thể sửa đơn thuốc");

        var allergyRepository = unitOfWork.Repository<PatientAllergy>();
        var allergies = await allergyRepository.ToListAsync(
            allergyRepository.Query().Where(a => a.PatientId == record.PatientId), cancellationToken);
        var allergenNames = allergies.Select(a => a.Allergen).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var medicineRepository = unitOfWork.Repository<Medicine>();
        var medicineIds = request.Items.Select(i => i.MedicineId).Distinct().ToList();
        var medicines = await medicineRepository.ToListAsync(
            medicineRepository.Query().Where(m => medicineIds.Contains(m.Id)), cancellationToken);
        var medicineById = medicines.ToDictionary(m => m.Id);

        foreach (var item in request.Items)
        {
            if (!medicineById.TryGetValue(item.MedicineId, out var medicine))
                throw new NotFoundException(nameof(Medicine), item.MedicineId);

            var hasAllergyMatch = medicine.ActiveIngredient is not null && allergenNames.Contains(medicine.ActiveIngredient);
            if (hasAllergyMatch && !item.AllergyConfirmed)
                throw new ValidationException("items", $"Thuốc '{medicine.Name}' có thể gây dị ứng theo tiền sử bệnh nhân — cần xác nhận trước khi kê");
        }

        var prescriptionRepository = unitOfWork.Repository<Prescription>();
        var prescription = await prescriptionRepository.FirstOrDefaultAsync(
            prescriptionRepository.Query().Where(p => p.MedicalRecordId == medicalRecordId), cancellationToken);

        var itemRepository = unitOfWork.Repository<PrescriptionItem>();

        if (prescription is null)
        {
            prescription = new Prescription { MedicalRecordId = medicalRecordId, DoctorId = doctorId, Note = request.Note };
            await prescriptionRepository.AddAsync(prescription, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken); // need prescription.Id before adding items
        }
        else
        {
            prescription.Note = request.Note;
            prescriptionRepository.Update(prescription);

            // "Lưu đơn thuốc" replaces the whole item list each time — simplest
            // correct semantics given prescriptions.medical_record_id is UNIQUE
            // (one prescription per visit, edited in place while still Draft).
            var existingItems = await itemRepository.ToListAsync(
                itemRepository.Query().Where(i => i.PrescriptionId == prescription.Id), cancellationToken);
            foreach (var existing in existingItems)
                itemRepository.Remove(existing);
        }

        foreach (var item in request.Items)
        {
            var medicine = medicineById[item.MedicineId];
            await itemRepository.AddAsync(new PrescriptionItem
            {
                PrescriptionId = prescription.Id,
                MedicineId = item.MedicineId,
                MedicineNameSnapshot = medicine.Name,
                UnitPriceSnapshot = medicine.Price,
                Quantity = item.Quantity,
                Dosage = item.Dosage,
                DurationDays = item.DurationDays,
                Instruction = item.Instruction,
            }, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await BuildPrescriptionDtoAsync(prescription, cancellationToken);
    }

    public async Task<InvoiceDto> FinalizeAsync(int id, CancellationToken cancellationToken = default)
    {
        var recordRepository = unitOfWork.Repository<MedicalRecord>();
        var record = await recordRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(MedicalRecord), id);

        if (record.Status != MedicalRecordStatus.Draft)
            throw new ConflictException("Bệnh án đã được chốt trước đó");

        if (string.IsNullOrWhiteSpace(record.Diagnosis))
            throw new ValidationException("diagnosis", "Phải nhập chẩn đoán trước khi chốt bệnh án");

        var mrsRepository = unitOfWork.Repository<MedicalRecordService>();
        var orderedServices = await mrsRepository.ToListAsync(
            mrsRepository.Query().Where(s => s.MedicalRecordId == id), cancellationToken);

        if (orderedServices.Any(s => s.Status is MedicalRecordServiceStatus.Ordered or MedicalRecordServiceStatus.InProgress))
            throw new ConflictException("Còn chỉ định cận lâm sàng chưa hoàn tất hoặc chưa huỷ");

        var appointmentRepository = unitOfWork.Repository<Appointment>();
        var appointment = await appointmentRepository.GetByIdAsync(record.AppointmentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Appointment), record.AppointmentId);

        var patient = await unitOfWork.Repository<Patient>().GetByIdAsync(record.PatientId, cancellationToken);

        var invoice = new Invoice
        {
            PatientId = record.PatientId,
            PatientName = patient?.FullName, // snapshot, same reasoning as invoice_items' own snapshots
            AppointmentId = record.AppointmentId,
            MedicalRecordId = record.Id,
            Status = InvoiceStatus.Unpaid,
        };
        var invoiceRepository = unitOfWork.Repository<Invoice>();
        await invoiceRepository.AddAsync(invoice, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken); // need invoice.Id for the code + item FKs

        invoice.InvoiceNo = $"HD{invoice.Id:D6}";

        var invoiceItemRepository = unitOfWork.Repository<InvoiceItem>();
        var invoiceItems = new List<InvoiceItem>();
        decimal total = 0;

        // Consultation fee — references neither a service nor a medicine, which is
        // exactly the case CK_invoice_items_at_most_one_ref was relaxed to allow.
        if (appointment.FeeSnapshot is > 0)
        {
            var feeItem = new InvoiceItem
            {
                InvoiceId = invoice.Id,
                Description = "Phí khám",
                Quantity = 1,
                UnitPrice = appointment.FeeSnapshot.Value,
                Amount = appointment.FeeSnapshot.Value,
            };
            invoiceItems.Add(feeItem);
            total += feeItem.Amount;
        }

        // "Chỉ định chỉ lên hoá đơn khi trạng thái Completed" — Cancelled ones are excluded.
        foreach (var svc in orderedServices.Where(s => s.Status == MedicalRecordServiceStatus.Completed))
        {
            var amount = svc.UnitPriceSnapshot * svc.Quantity;
            var item = new InvoiceItem
            {
                InvoiceId = invoice.Id,
                MedicalRecordServiceId = svc.Id,
                Quantity = svc.Quantity,
                UnitPrice = svc.UnitPriceSnapshot,
                Amount = amount,
            };
            invoiceItems.Add(item);
            total += amount;
        }

        var prescriptionRepository = unitOfWork.Repository<Prescription>();
        var prescription = await prescriptionRepository.FirstOrDefaultAsync(
            prescriptionRepository.Query().Where(p => p.MedicalRecordId == id), cancellationToken);

        if (prescription is not null)
        {
            var itemRepository = unitOfWork.Repository<PrescriptionItem>();
            var items = await itemRepository.ToListAsync(itemRepository.Query().Where(i => i.PrescriptionId == prescription.Id), cancellationToken);
            foreach (var item in items)
            {
                var unitPrice = item.UnitPriceSnapshot ?? 0;
                var amount = unitPrice * item.Quantity;
                var invoiceItem = new InvoiceItem
                {
                    InvoiceId = invoice.Id,
                    MedicineId = item.MedicineId,
                    Description = item.MedicineNameSnapshot,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    Amount = amount,
                };
                invoiceItems.Add(invoiceItem);
                total += amount;
            }
        }

        foreach (var item in invoiceItems)
            await invoiceItemRepository.AddAsync(item, cancellationToken);

        invoice.TotalAmount = total;
        invoiceRepository.Update(invoice);

        record.Status = MedicalRecordStatus.Finalized;
        record.FinalizedAt = DateTime.UtcNow;
        record.UpdatedAt = DateTime.UtcNow;
        recordRepository.Update(record);

        var fromStatus = appointment.Status;
        appointment.Status = AppointmentStatus.Completed;
        appointment.CompletedAt = DateTime.UtcNow;
        appointment.UpdatedAt = DateTime.UtcNow;
        appointmentRepository.Update(appointment);
        await unitOfWork.Repository<AppointmentStatusHistory>().AddAsync(new AppointmentStatusHistory
        {
            AppointmentId = appointment.Id,
            FromStatus = fromStatus,
            ToStatus = AppointmentStatus.Completed,
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return invoice.ToDto(invoiceItems.Select(i => i.ToDto()).ToList(), []);
    }

    private async Task UpsertVitalsAsync(int medicalRecordId, VitalsDto vitals, CancellationToken cancellationToken)
    {
        var vitalsRepository = unitOfWork.Repository<PatientVitals>();
        var existing = await vitalsRepository.FirstOrDefaultAsync(
            vitalsRepository.Query().Where(v => v.MedicalRecordId == medicalRecordId), cancellationToken);

        if (existing is null)
        {
            await vitalsRepository.AddAsync(new PatientVitals
            {
                MedicalRecordId = medicalRecordId,
                Temperature = vitals.Temperature,
                Pulse = vitals.Pulse,
                BloodPressure = vitals.BloodPressure,
                Weight = vitals.Weight,
                Height = vitals.Height,
            }, cancellationToken);
        }
        else
        {
            existing.Temperature = vitals.Temperature;
            existing.Pulse = vitals.Pulse;
            existing.BloodPressure = vitals.BloodPressure;
            existing.Weight = vitals.Weight;
            existing.Height = vitals.Height;
            vitalsRepository.Update(existing);
        }
    }

    private async Task<MedicalRecordDto> BuildDtoAsync(MedicalRecord record, CancellationToken cancellationToken)
    {
        var patient = await unitOfWork.Repository<Patient>().GetByIdAsync(record.PatientId, cancellationToken);
        var doctor = await unitOfWork.Repository<Doctor>().GetByIdAsync(record.DoctorId, cancellationToken);

        var vitalsRepository = unitOfWork.Repository<PatientVitals>();
        var vitals = await vitalsRepository.FirstOrDefaultAsync(
            vitalsRepository.Query().Where(v => v.MedicalRecordId == record.Id), cancellationToken);

        var mrsRepository = unitOfWork.Repository<MedicalRecordService>();
        var services = await mrsRepository.ToListAsync(mrsRepository.Query().Where(s => s.MedicalRecordId == record.Id), cancellationToken);
        var serviceDtos = await BuildServiceDtosAsync(services, cancellationToken);

        var prescriptionRepository = unitOfWork.Repository<Prescription>();
        var prescription = await prescriptionRepository.FirstOrDefaultAsync(
            prescriptionRepository.Query().Where(p => p.MedicalRecordId == record.Id), cancellationToken);
        var prescriptionDto = prescription is null ? null : await BuildPrescriptionDtoAsync(prescription, cancellationToken);

        return record.ToDto(patient?.FullName ?? "", doctor?.FullName ?? "", vitals?.ToDto(), serviceDtos, prescriptionDto);
    }

    private async Task<List<MedicalRecordServiceDto>> BuildServiceDtosAsync(List<MedicalRecordService> services, CancellationToken cancellationToken)
    {
        if (services.Count == 0)
            return [];

        var serviceIds = services.Select(s => s.ServiceId).Distinct().ToList();
        var serviceRepository = unitOfWork.Repository<Service>();
        var serviceEntities = await serviceRepository.ToListAsync(serviceRepository.Query().Where(s => serviceIds.Contains(s.Id)), cancellationToken);
        var serviceNameById = serviceEntities.ToDictionary(s => s.Id, s => s.Name);

        var mrsIds = services.Select(s => s.Id).ToList();
        var labResultRepository = unitOfWork.Repository<LabResult>();
        var labResults = await labResultRepository.ToListAsync(
            labResultRepository.Query().Where(l => mrsIds.Contains(l.MedicalRecordServiceId)), cancellationToken);
        var labResultByMrsId = labResults.ToDictionary(l => l.MedicalRecordServiceId);

        return services
            .Select(s => s.ToDto(
                serviceNameById.GetValueOrDefault(s.ServiceId, ""),
                labResultByMrsId.TryGetValue(s.Id, out var result) ? result.ToDto() : null))
            .ToList();
    }

    private async Task<PrescriptionDto> BuildPrescriptionDtoAsync(Prescription prescription, CancellationToken cancellationToken)
    {
        var itemRepository = unitOfWork.Repository<PrescriptionItem>();
        var items = await itemRepository.ToListAsync(itemRepository.Query().Where(i => i.PrescriptionId == prescription.Id), cancellationToken);

        return prescription.ToDto(items.Select(i => i.ToDto()).ToList());
    }
}

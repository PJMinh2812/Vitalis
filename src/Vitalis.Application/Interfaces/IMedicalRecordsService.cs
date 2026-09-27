using Vitalis.Application.Common;
using Vitalis.Application.DTOs.Billing;
using Vitalis.Application.DTOs.MedicalRecords;

namespace Vitalis.Application.Interfaces;

public interface IMedicalRecordsService
{
    // Bridges Appointment (bước 6) -> MedicalRecord (bước 8): CheckedIn -> InProgress,
    // creates the Draft record if one doesn't already exist for this appointment.
    Task<MedicalRecordDto> StartExamAsync(int appointmentId, CancellationToken cancellationToken = default);

    Task<MedicalRecordDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    // VC-05: only Finalized records, newest first.
    Task<PagedResult<MedicalRecordDto>> GetPatientHistoryAsync(int patientId, int page, int pageSize, CancellationToken cancellationToken = default);

    // VC-13 #1 — only while Status == Draft.
    Task<MedicalRecordDto> SaveDraftAsync(int id, MedicalRecordRequest request, CancellationToken cancellationToken = default);

    // VC-14 #1
    Task<MedicalRecordServiceDto> OrderServiceAsync(int medicalRecordId, OrderServiceRequest request, CancellationToken cancellationToken = default);

    // VC-14 #2 — only while the ordered service is still Ordered (not yet performed).
    Task CancelServiceAsync(int medicalRecordServiceId, CancellationToken cancellationToken = default);

    // VC-14 #3
    Task<MedicalRecordServiceDto> EnterResultAsync(int medicalRecordServiceId, LabResultRequest request, int performedBy, CancellationToken cancellationToken = default);

    // VC-15 #2 — upsert (replace-all-items) since prescriptions.medical_record_id is UNIQUE.
    // Throws ValidationException if an item matches a patient allergy and isn't confirmed.
    Task<PrescriptionDto> SavePrescriptionAsync(int medicalRecordId, PrescriptionRequest request, int doctorId, CancellationToken cancellationToken = default);

    // VC-13 #4 — point of no return: locks the record, requires diagnosis and every
    // ordered service to be Completed/Cancelled, creates the Invoice (fee + services +
    // medicines), and sets the appointment to Completed.
    Task<InvoiceDto> FinalizeAsync(int id, CancellationToken cancellationToken = default);
}

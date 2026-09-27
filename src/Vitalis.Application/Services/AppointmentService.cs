using Vitalis.Application.Common;
using Vitalis.Application.DTOs.Appointments;
using Vitalis.Application.DTOs.Doctors;
using Vitalis.Application.Exceptions;
using Vitalis.Application.Interfaces;
using Vitalis.Domain.Entities.Scheduling;
using Vitalis.Domain.Enums;

namespace Vitalis.Application.Services;

public class AppointmentService(IUnitOfWork unitOfWork, IDoctorService doctorService) : IAppointmentService
{
    private const int MinCancelHoursBefore = 2;

    public async Task<AppointmentDto> BookAsync(CreateAppointmentRequest request, int? createdBy, CancellationToken cancellationToken = default)
    {
        _ = await unitOfWork.Repository<Patient>().GetByIdAsync(request.PatientId, cancellationToken)
            ?? throw new NotFoundException(nameof(Patient), request.PatientId);

        var doctor = await unitOfWork.Repository<Doctor>().GetByIdAsync(request.DoctorId, cancellationToken)
            ?? throw new NotFoundException(nameof(Doctor), request.DoctorId);

        if (!doctor.IsActive)
            throw new ConflictException("Bác sĩ hiện không nhận lịch hẹn");

        var date = DateOnly.FromDateTime(request.StartTime);
        var matchedSlot = await MatchAvailableSlotAsync(request.DoctorId, date, TimeOnly.FromDateTime(request.StartTime), cancellationToken);

        var appointment = new Appointment
        {
            PatientId = request.PatientId,
            DoctorId = request.DoctorId,
            StartTime = date.ToDateTime(matchedSlot.StartTime),
            EndTime = date.ToDateTime(matchedSlot.EndTime),
            Status = AppointmentStatus.Pending,
            Source = request.Source,
            Reason = request.Reason,
            FeeSnapshot = doctor.ConsultationFee,
            CreatedBy = createdBy,
        };

        var appointmentRepository = unitOfWork.Repository<Appointment>();
        await appointmentRepository.AddAsync(appointment, cancellationToken);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConflictException)
        {
            // The pre-check above passed, but someone else grabbed the exact same
            // slot in the meantime — UX_appointments_doctor_slot (the filtered
            // unique index) is the real enforcement; this just turns the DB
            // rejection into the same clean 409 + refreshed slot list VC-03 wants.
            throw new SlotUnavailableException(await GetAvailableOnlyAsync(request.DoctorId, date, cancellationToken));
        }

        // Two round trips, same reason as PatientCode: the code is derived from
        // the IDENTITY value SQL Server just assigned, so it can never collide.
        appointment.AppointmentCode = $"LH{appointment.Id:D6}";
        await RecordHistoryAsync(appointment.Id, null, AppointmentStatus.Pending, createdBy, null, cancellationToken);
        await unitOfWork.Repository<Notification>().AddAsync(new Notification
        {
            AppointmentId = appointment.Id,
            Channel = NotificationChannel.Sms,
        }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await BuildDtoAsync(appointment, cancellationToken);
    }

    public async Task<PagedResult<AppointmentDto>> SearchAsync(int? patientId, int? doctorId, DateOnly? date, AppointmentStatus? status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var repository = unitOfWork.Repository<Appointment>();
        var query = repository.Query();

        if (patientId is not null) query = query.Where(a => a.PatientId == patientId);
        if (doctorId is not null) query = query.Where(a => a.DoctorId == doctorId);
        if (status is not null) query = query.Where(a => a.Status == status);

        if (date is not null)
        {
            var dayStart = date.Value.ToDateTime(TimeOnly.MinValue);
            var dayEnd = dayStart.AddDays(1);
            query = query.Where(a => a.StartTime >= dayStart && a.StartTime < dayEnd);
        }

        query = query.OrderBy(a => a.StartTime);

        var paged = await repository.GetPagedAsync(query, page, pageSize, cancellationToken);
        var items = await ToDtoBatchAsync(paged.Items, cancellationToken);

        return new PagedResult<AppointmentDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }

    public async Task<AppointmentDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var appointment = await unitOfWork.Repository<Appointment>().GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Appointment), id);

        return await BuildDtoAsync(appointment, cancellationToken);
    }

    public async Task<AppointmentDto> CancelAsync(int id, string cancelReason, int? changedBy, CancellationToken cancellationToken = default)
    {
        var repository = unitOfWork.Repository<Appointment>();
        var appointment = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Appointment), id);

        if (appointment.Status >= AppointmentStatus.Completed)
            throw new ConflictException("Lịch hẹn đã kết thúc, không thể huỷ");

        // Same UTC+7 approximation as DoctorService.GetAvailableSlotsAsync — see
        // the comment there and learning-notes/05 for why this is a known simplification.
        var nowLocal = DateTime.UtcNow.AddHours(7);
        if (appointment.StartTime <= nowLocal.AddHours(MinCancelHoursBefore))
            throw new ConflictException($"Không thể huỷ lịch hẹn trong vòng {MinCancelHoursBefore} giờ trước giờ khám");

        var fromStatus = appointment.Status;
        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancelReason = cancelReason;
        appointment.UpdatedAt = DateTime.UtcNow;
        repository.Update(appointment);

        await RecordHistoryAsync(appointment.Id, fromStatus, AppointmentStatus.Cancelled, changedBy, cancelReason, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await BuildDtoAsync(appointment, cancellationToken);
    }

    public async Task<AppointmentDto> RescheduleAsync(int id, DateTime newStartTime, int? changedBy, CancellationToken cancellationToken = default)
    {
        var repository = unitOfWork.Repository<Appointment>();
        var appointment = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Appointment), id);

        if (appointment.Status >= AppointmentStatus.Completed)
            throw new ConflictException("Lịch hẹn đã kết thúc, không thể đổi giờ");

        var date = DateOnly.FromDateTime(newStartTime);
        var matchedSlot = await MatchAvailableSlotAsync(appointment.DoctorId, date, TimeOnly.FromDateTime(newStartTime), cancellationToken);

        var oldStartTime = appointment.StartTime;
        appointment.StartTime = date.ToDateTime(matchedSlot.StartTime);
        appointment.EndTime = date.ToDateTime(matchedSlot.EndTime);
        appointment.UpdatedAt = DateTime.UtcNow;
        repository.Update(appointment);

        try
        {
            await RecordHistoryAsync(appointment.Id, appointment.Status, appointment.Status, changedBy,
                $"Đổi giờ từ {oldStartTime:HH:mm dd/MM} sang {appointment.StartTime:HH:mm dd/MM}", cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConflictException)
        {
            throw new SlotUnavailableException(await GetAvailableOnlyAsync(appointment.DoctorId, date, cancellationToken));
        }

        return await BuildDtoAsync(appointment, cancellationToken);
    }

    public async Task<AppointmentDto> CheckInAsync(int id, int? changedBy, CancellationToken cancellationToken = default)
    {
        var repository = unitOfWork.Repository<Appointment>();
        var appointment = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Appointment), id);

        if (appointment.Status is not (AppointmentStatus.Pending or AppointmentStatus.Confirmed))
            throw new ConflictException("Chỉ có thể check-in lịch hẹn đang ở trạng thái chờ đến");

        var dayStart = appointment.StartTime.Date;
        var dayEnd = dayStart.AddDays(1);

        // Best-effort sequential numbering (MAX+1) — a narrow race window remains
        // if two reception counters check in for the same doctor at the exact
        // same instant; acceptable for a cosmetic queue number (see learning-notes),
        // unlike the appointment slot itself, which is protected by a DB unique index.
        var todaysCheckedIn = await repository.ToListAsync(
            repository.Query().Where(a => a.DoctorId == appointment.DoctorId && a.StartTime >= dayStart && a.StartTime < dayEnd && a.QueueNumber != null),
            cancellationToken);
        var nextQueueNumber = todaysCheckedIn.Count == 0 ? 1 : todaysCheckedIn.Max(a => a.QueueNumber!.Value) + 1;

        var fromStatus = appointment.Status;
        appointment.Status = AppointmentStatus.CheckedIn;
        appointment.QueueNumber = nextQueueNumber;
        appointment.CheckedInAt = DateTime.UtcNow;
        appointment.UpdatedAt = DateTime.UtcNow;
        repository.Update(appointment);

        await RecordHistoryAsync(appointment.Id, fromStatus, AppointmentStatus.CheckedIn, changedBy, null, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await BuildDtoAsync(appointment, cancellationToken);
    }

    public async Task<AppointmentDto> MarkNoShowAsync(int id, string? note, int? changedBy, CancellationToken cancellationToken = default)
    {
        var repository = unitOfWork.Repository<Appointment>();
        var appointment = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Appointment), id);

        if (appointment.Status is not (AppointmentStatus.Pending or AppointmentStatus.Confirmed))
            throw new ConflictException("Chỉ có thể đánh dấu không đến khi lịch hẹn đang ở trạng thái chờ đến");

        var fromStatus = appointment.Status;
        appointment.Status = AppointmentStatus.NoShow;
        appointment.Note = note;
        appointment.UpdatedAt = DateTime.UtcNow;
        repository.Update(appointment);

        await RecordHistoryAsync(appointment.Id, fromStatus, AppointmentStatus.NoShow, changedBy, note, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await BuildDtoAsync(appointment, cancellationToken);
    }

    // The core ⭐ rule: never trust a freeform time from the client — the
    // requested StartTime must land exactly on a slot GetAvailableSlotsAsync
    // would currently offer as available.
    private async Task<SlotDto> MatchAvailableSlotAsync(int doctorId, DateOnly date, TimeOnly requestedStart, CancellationToken cancellationToken)
    {
        var slots = await doctorService.GetAvailableSlotsAsync(doctorId, date, cancellationToken);
        var matched = slots.FirstOrDefault(s => s.StartTime == requestedStart);

        if (matched is null || !matched.IsAvailable)
            throw new SlotUnavailableException(slots.Where(s => s.IsAvailable).ToList());

        return matched;
    }

    private async Task<IReadOnlyList<SlotDto>> GetAvailableOnlyAsync(int doctorId, DateOnly date, CancellationToken cancellationToken)
    {
        var slots = await doctorService.GetAvailableSlotsAsync(doctorId, date, cancellationToken);
        return slots.Where(s => s.IsAvailable).ToList();
    }

    private async Task RecordHistoryAsync(int appointmentId, AppointmentStatus? fromStatus, AppointmentStatus toStatus, int? changedBy, string? reason, CancellationToken cancellationToken)
    {
        await unitOfWork.Repository<AppointmentStatusHistory>().AddAsync(new AppointmentStatusHistory
        {
            AppointmentId = appointmentId,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ChangedBy = changedBy,
            Reason = reason,
        }, cancellationToken);
    }

    private async Task<AppointmentDto> BuildDtoAsync(Appointment appointment, CancellationToken cancellationToken)
    {
        var patient = await unitOfWork.Repository<Patient>().GetByIdAsync(appointment.PatientId, cancellationToken);
        var doctor = await unitOfWork.Repository<Doctor>().GetByIdAsync(appointment.DoctorId, cancellationToken);

        return appointment.ToDto(patient?.FullName ?? "", doctor?.FullName ?? "");
    }

    // In-memory join instead of EF Core Include — same pattern as UserService/DoctorService.
    private async Task<List<AppointmentDto>> ToDtoBatchAsync(IReadOnlyList<Appointment> appointments, CancellationToken cancellationToken)
    {
        var patientIds = appointments.Select(a => a.PatientId).Distinct().ToList();
        var patientRepository = unitOfWork.Repository<Patient>();
        var patients = await patientRepository.ToListAsync(patientRepository.Query().Where(p => patientIds.Contains(p.Id)), cancellationToken);
        var patientNameById = patients.ToDictionary(p => p.Id, p => p.FullName);

        var doctorIds = appointments.Select(a => a.DoctorId).Distinct().ToList();
        var doctorRepository = unitOfWork.Repository<Doctor>();
        var doctors = await doctorRepository.ToListAsync(doctorRepository.Query().Where(d => doctorIds.Contains(d.Id)), cancellationToken);
        var doctorNameById = doctors.ToDictionary(d => d.Id, d => d.FullName);

        return appointments
            .Select(a => a.ToDto(patientNameById.GetValueOrDefault(a.PatientId, ""), doctorNameById.GetValueOrDefault(a.DoctorId, "")))
            .ToList();
    }
}

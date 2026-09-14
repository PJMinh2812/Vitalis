using Vitalis.Application.DTOs.Doctors;
using Vitalis.Application.Exceptions;
using Vitalis.Application.Interfaces;
using Vitalis.Domain.Entities.Scheduling;
using Vitalis.Domain.Enums;

namespace Vitalis.Application.Services;

public class DoctorTimeOffService(IUnitOfWork unitOfWork) : IDoctorTimeOffService
{
    public async Task<IReadOnlyList<DoctorTimeOffDto>> GetByDoctorAsync(int? doctorId, CancellationToken cancellationToken = default)
    {
        var repository = unitOfWork.Repository<DoctorTimeOff>();
        var query = repository.Query();

        if (doctorId is not null)
            query = query.Where(t => t.DoctorId == doctorId);

        query = query.OrderByDescending(t => t.StartAt);

        var items = await repository.ToListAsync(query, cancellationToken);
        return items.Select(t => t.ToDto()).ToList();
    }

    public async Task<CreateTimeOffResult> CreateAsync(CreateTimeOffRequest request, CancellationToken cancellationToken = default)
    {
        if (request.DoctorId is not null)
        {
            _ = await unitOfWork.Repository<Doctor>().GetByIdAsync(request.DoctorId.Value, cancellationToken)
                ?? throw new NotFoundException(nameof(Doctor), request.DoctorId.Value);
        }

        // Heuristic, not a form field: a range that lands exactly on midnight-to-
        // midnight boundaries reads as whole day(s) off; anything else is a partial day.
        var isFullDay = request.StartAt.TimeOfDay == TimeSpan.Zero && request.EndAt.TimeOfDay == TimeSpan.Zero;

        var timeOff = new DoctorTimeOff
        {
            DoctorId = request.DoctorId,
            Type = request.Type,
            IsFullDay = isFullDay,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            Reason = request.Reason,
        };
        var timeOffRepository = unitOfWork.Repository<DoctorTimeOff>();
        await timeOffRepository.AddAsync(timeOff, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var conflicts = await FindConflictingAppointmentsAsync(request.DoctorId, request.StartAt, request.EndAt, cancellationToken);

        return new CreateTimeOffResult(timeOff.ToDto(), conflicts);
    }

    // Does not block creation — VC-20 wants the list back so reception can
    // reschedule these appointments, not a hard failure.
    private async Task<IReadOnlyList<ConflictingAppointmentDto>> FindConflictingAppointmentsAsync(
        int? doctorId, DateTime startAt, DateTime endAt, CancellationToken cancellationToken)
    {
        var appointmentRepository = unitOfWork.Repository<Appointment>();
        var query = appointmentRepository.Query().Where(a =>
            a.StartTime < endAt && a.EndTime > startAt && a.Status < AppointmentStatus.Cancelled);

        if (doctorId is not null)
            query = query.Where(a => a.DoctorId == doctorId);

        var conflicts = await appointmentRepository.ToListAsync(query, cancellationToken);
        if (conflicts.Count == 0)
            return [];

        var patientIds = conflicts.Select(a => a.PatientId).Distinct().ToList();
        var patientRepository = unitOfWork.Repository<Patient>();
        var patients = await patientRepository.ToListAsync(
            patientRepository.Query().Where(p => patientIds.Contains(p.Id)), cancellationToken);
        var patientNameById = patients.ToDictionary(p => p.Id, p => p.FullName);

        return conflicts.Select(a => new ConflictingAppointmentDto(
            a.Id, a.AppointmentCode, a.StartTime, a.EndTime, patientNameById.GetValueOrDefault(a.PatientId, ""))).ToList();
    }
}

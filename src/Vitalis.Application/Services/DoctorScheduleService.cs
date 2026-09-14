using Vitalis.Application.DTOs.Doctors;
using Vitalis.Application.Exceptions;
using Vitalis.Application.Interfaces;
using Vitalis.Domain.Entities.Scheduling;

namespace Vitalis.Application.Services;

public class DoctorScheduleService(IUnitOfWork unitOfWork) : IDoctorScheduleService
{
    public async Task<IReadOnlyList<DoctorScheduleDto>> GetByDoctorAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        var repository = unitOfWork.Repository<DoctorSchedule>();
        var query = repository.Query()
            .Where(s => s.DoctorId == doctorId)
            .OrderBy(s => s.DayOfWeek).ThenBy(s => s.StartTime);

        var schedules = await repository.ToListAsync(query, cancellationToken);
        return schedules.Select(s => s.ToDto()).ToList();
    }

    public async Task<DoctorScheduleDto> CreateAsync(ScheduleRequest request, CancellationToken cancellationToken = default)
    {
        var doctorRepository = unitOfWork.Repository<Doctor>();
        _ = await doctorRepository.GetByIdAsync(request.DoctorId, cancellationToken)
            ?? throw new NotFoundException(nameof(Doctor), request.DoctorId);

        var dayOfWeek = (DayOfWeek)request.DayOfWeek;
        var scheduleRepository = unitOfWork.Repository<DoctorSchedule>();

        // Backstop for UX_doctor_schedules_slot — a friendly 409 instead of a raw
        // SQL unique-violation. Admin-only, low-concurrency operation, so a plain
        // pre-check (no transaction lock) is enough, unlike appointment booking.
        var duplicate = await scheduleRepository.FirstOrDefaultAsync(
            scheduleRepository.Query().Where(s =>
                s.DoctorId == request.DoctorId &&
                s.DayOfWeek == dayOfWeek &&
                s.StartTime == request.StartTime),
            cancellationToken);
        if (duplicate is not null)
            throw new ConflictException("Bác sĩ đã có lịch làm việc bắt đầu vào giờ này trong ngày đã chọn");

        var schedule = new DoctorSchedule
        {
            DoctorId = request.DoctorId,
            DayOfWeek = dayOfWeek,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            BreakStart = request.BreakStart,
            BreakEnd = request.BreakEnd,
            SlotMinutes = request.SlotMinutes,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
        };
        await scheduleRepository.AddAsync(schedule, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return schedule.ToDto();
    }
}

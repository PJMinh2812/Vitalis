using Vitalis.Application.Common;
using Vitalis.Application.DTOs.Doctors;
using Vitalis.Application.Exceptions;
using Vitalis.Application.Interfaces;
using Vitalis.Domain.Entities.Auth;
using Vitalis.Domain.Entities.Scheduling;
using Vitalis.Domain.Enums;

namespace Vitalis.Application.Services;

public class DoctorService(IUnitOfWork unitOfWork, IUserService userService) : IDoctorService
{
    public async Task<PagedResult<DoctorDto>> SearchAsync(string? keyword, int? specialtyId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var repository = unitOfWork.Repository<Doctor>();
        var query = repository.Query().Where(d => d.IsActive);

        if (specialtyId is not null)
            query = query.Where(d => d.SpecialtyId == specialtyId);

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(d => d.FullName.StartsWith(keyword));

        query = query.OrderBy(d => d.FullName);

        var paged = await repository.GetPagedAsync(query, page, pageSize, cancellationToken);
        var items = await ToDtoBatchAsync(paged.Items, cancellationToken);

        return new PagedResult<DoctorDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }

    public async Task<DoctorDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var doctor = await unitOfWork.Repository<Doctor>().GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Doctor), id);

        var specialty = await unitOfWork.Repository<Specialty>().GetByIdAsync(doctor.SpecialtyId, cancellationToken);
        var user = await unitOfWork.Repository<User>().GetByIdAsync(doctor.UserId, cancellationToken);

        return doctor.ToDto(specialty?.Name ?? "", user?.Username ?? "");
    }

    public async Task<DoctorDto> CreateAsync(DoctorRequest request, CancellationToken cancellationToken = default)
    {
        _ = await unitOfWork.Repository<Specialty>().GetByIdAsync(request.SpecialtyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Specialty), request.SpecialtyId);

        var userId = await userService.CreateUserAsync(
            request.Username, request.Password, request.FullName, null, null, ["Doctor"], cancellationToken);

        var doctor = new Doctor
        {
            UserId = userId,
            SpecialtyId = request.SpecialtyId,
            FullName = request.FullName,
            Title = request.Title,
            LicenseNumber = request.LicenseNumber,
            Room = request.Room,
            ConsultationFee = request.ConsultationFee,
            MaxPatientsPerDay = request.MaxPatientsPerDay,
        };
        await unitOfWork.Repository<Doctor>().AddAsync(doctor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(doctor.Id, cancellationToken);
    }

    public async Task<DoctorDto> UpdateAsync(int id, UpdateDoctorRequest request, CancellationToken cancellationToken = default)
    {
        var repository = unitOfWork.Repository<Doctor>();
        var doctor = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Doctor), id);

        if (request.SpecialtyId != doctor.SpecialtyId)
        {
            _ = await unitOfWork.Repository<Specialty>().GetByIdAsync(request.SpecialtyId, cancellationToken)
                ?? throw new NotFoundException(nameof(Specialty), request.SpecialtyId);
        }

        doctor.FullName = request.FullName;
        doctor.Title = request.Title;
        doctor.SpecialtyId = request.SpecialtyId;
        doctor.LicenseNumber = request.LicenseNumber;
        doctor.Room = request.Room;
        doctor.ConsultationFee = request.ConsultationFee;
        doctor.MaxPatientsPerDay = request.MaxPatientsPerDay;
        doctor.UpdatedAt = DateTime.UtcNow;

        repository.Update(doctor);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var repository = unitOfWork.Repository<Doctor>();
        var doctor = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Doctor), id);

        var appointmentRepository = unitOfWork.Repository<Appointment>();
        var hasUnfinishedAppointments = await appointmentRepository.FirstOrDefaultAsync(
            appointmentRepository.Query().Where(a => a.DoctorId == id && a.Status < AppointmentStatus.Completed),
            cancellationToken) is not null;

        if (hasUnfinishedAppointments)
            throw new ConflictException("Bác sĩ còn lịch hẹn chưa khám — hãy chuyển lịch trước khi ngừng hoạt động");

        doctor.IsActive = false;
        doctor.UpdatedAt = DateTime.UtcNow;
        repository.Update(doctor);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SlotDto>> GetAvailableSlotsAsync(int doctorId, DateOnly date, CancellationToken cancellationToken = default)
    {
        var doctor = await unitOfWork.Repository<Doctor>().GetByIdAsync(doctorId, cancellationToken)
            ?? throw new NotFoundException(nameof(Doctor), doctorId);

        if (!doctor.IsActive)
            return [];

        var scheduleRepository = unitOfWork.Repository<DoctorSchedule>();
        var schedules = await scheduleRepository.ToListAsync(
            scheduleRepository.Query().Where(s =>
                s.DoctorId == doctorId &&
                s.IsActive &&
                s.DayOfWeek == date.DayOfWeek &&
                s.EffectiveFrom <= date &&
                (s.EffectiveTo == null || s.EffectiveTo >= date)),
            cancellationToken);

        if (schedules.Count == 0)
            return [];

        var dayStart = date.ToDateTime(TimeOnly.MinValue);
        var dayEnd = dayStart.AddDays(1);

        var timeOffRepository = unitOfWork.Repository<DoctorTimeOff>();
        var timeOffs = await timeOffRepository.ToListAsync(
            timeOffRepository.Query().Where(t =>
                (t.DoctorId == doctorId || t.DoctorId == null) &&
                t.StartAt < dayEnd && t.EndAt > dayStart),
            cancellationToken);

        var appointmentRepository = unitOfWork.Repository<Appointment>();
        var appointments = await appointmentRepository.ToListAsync(
            appointmentRepository.Query().Where(a =>
                a.DoctorId == doctorId &&
                a.Status < AppointmentStatus.Cancelled &&
                a.StartTime < dayEnd && a.EndTime > dayStart),
            cancellationToken);

        // Clinic runs on UTC+7; there's no shared TimeZone helper in the codebase
        // yet (see CLAUDE.md's "converted to UTC+7 in the application layer"), so
        // this is approximated inline the same way other modules keep things simple.
        var nowLocal = DateTime.UtcNow.AddHours(7);
        var isToday = date == DateOnly.FromDateTime(nowLocal);
        var nowTimeOnly = TimeOnly.FromDateTime(nowLocal);

        var slots = new List<SlotDto>();

        foreach (var schedule in schedules)
        {
            var slotSpan = TimeSpan.FromMinutes(schedule.SlotMinutes);
            var slotStart = schedule.StartTime;

            while (slotStart.Add(slotSpan) <= schedule.EndTime)
            {
                var slotEnd = slotStart.Add(slotSpan);

                var duringBreak = schedule.BreakStart is not null && schedule.BreakEnd is not null
                    && slotStart < schedule.BreakEnd && slotEnd > schedule.BreakStart;
                var isPast = isToday && slotStart <= nowTimeOnly;

                if (duringBreak || isPast)
                {
                    slotStart = slotEnd;
                    continue;
                }

                var slotStartAt = date.ToDateTime(slotStart);
                var slotEndAt = date.ToDateTime(slotEnd);

                var onTimeOff = timeOffs.Any(t => t.StartAt < slotEndAt && t.EndAt > slotStartAt);
                var isBooked = appointments.Any(a => a.StartTime < slotEndAt && a.EndTime > slotStartAt);

                slots.Add(new SlotDto(slotStart, slotEnd, !onTimeOff && !isBooked));
                slotStart = slotEnd;
            }
        }

        return slots.OrderBy(s => s.StartTime).ToList();
    }

    // Batch-loads Specialty/User for a page of doctors and joins in memory
    // instead of N+1 querying per doctor (Application deliberately has no EF
    // Core Include — same pattern as UserService.SearchAsync).
    private async Task<List<DoctorDto>> ToDtoBatchAsync(IReadOnlyList<Doctor> doctors, CancellationToken cancellationToken)
    {
        var specialtyIds = doctors.Select(d => d.SpecialtyId).Distinct().ToList();
        var specialtyRepository = unitOfWork.Repository<Specialty>();
        var specialties = await specialtyRepository.ToListAsync(
            specialtyRepository.Query().Where(s => specialtyIds.Contains(s.Id)), cancellationToken);
        var specialtyNameById = specialties.ToDictionary(s => s.Id, s => s.Name);

        var userIds = doctors.Select(d => d.UserId).Distinct().ToList();
        var userRepository = unitOfWork.Repository<User>();
        var users = await userRepository.ToListAsync(
            userRepository.Query().Where(u => userIds.Contains(u.Id)), cancellationToken);
        var usernameById = users.ToDictionary(u => u.Id, u => u.Username);

        return doctors.Select(d => d.ToDto(specialtyNameById.GetValueOrDefault(d.SpecialtyId, ""), usernameById.GetValueOrDefault(d.UserId, ""))).ToList();
    }
}

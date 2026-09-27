using Vitalis.Application.DTOs.Doctors;

namespace Vitalis.Application.Interfaces;

public interface IDoctorScheduleService
{
    Task<IReadOnlyList<DoctorScheduleDto>> GetByDoctorAsync(int doctorId, CancellationToken cancellationToken = default);

    Task<DoctorScheduleDto> CreateAsync(ScheduleRequest request, CancellationToken cancellationToken = default);
}

using Vitalis.Application.Common;
using Vitalis.Application.DTOs.Doctors;

namespace Vitalis.Application.Interfaces;

public interface IDoctorService
{
    Task<PagedResult<DoctorDto>> SearchAsync(string? keyword, int? specialtyId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<DoctorDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    // VC-19: creates the login account (auth.users + Doctor role) and the doctor profile.
    Task<DoctorDto> CreateAsync(DoctorRequest request, CancellationToken cancellationToken = default);

    Task<DoctorDto> UpdateAsync(int id, UpdateDoctorRequest request, CancellationToken cancellationToken = default);

    // VC-19: blocked if the doctor still has non-completed appointments.
    Task DeactivateAsync(int id, CancellationToken cancellationToken = default);

    // VC-03: cuts the doctor's working hours into slots, removing lunch break,
    // time-off, past slots, and marking already-booked slots unavailable.
    Task<IReadOnlyList<SlotDto>> GetAvailableSlotsAsync(int doctorId, DateOnly date, CancellationToken cancellationToken = default);
}

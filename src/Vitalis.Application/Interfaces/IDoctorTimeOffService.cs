using Vitalis.Application.DTOs.Doctors;

namespace Vitalis.Application.Interfaces;

public interface IDoctorTimeOffService
{
    // doctorId == null lists every time-off (admin table); a value scopes to one doctor.
    Task<IReadOnlyList<DoctorTimeOffDto>> GetByDoctorAsync(int? doctorId, CancellationToken cancellationToken = default);

    Task<CreateTimeOffResult> CreateAsync(CreateTimeOffRequest request, CancellationToken cancellationToken = default);
}

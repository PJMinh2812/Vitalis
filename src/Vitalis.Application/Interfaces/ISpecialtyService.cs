using Vitalis.Application.DTOs.Doctors;

namespace Vitalis.Application.Interfaces;

public interface ISpecialtyService
{
    // activeOnly: null/false returns everything (admin table), true filters to
    // IsActive (VC-03 booking dropdown).
    Task<IReadOnlyList<SpecialtyDto>> GetAllAsync(bool? activeOnly, CancellationToken cancellationToken = default);

    Task<SpecialtyDto> CreateAsync(CreateSpecialtyRequest request, CancellationToken cancellationToken = default);
}

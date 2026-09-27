using Vitalis.Application.Common;
using Vitalis.Application.DTOs.Patients;

namespace Vitalis.Application.Interfaces;

public interface IPatientService
{
    Task<PatientDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PagedResult<PatientDto>> SearchAsync(string? keyword, int page, int pageSize, CancellationToken cancellationToken = default);

    // VC-02: pre-login lookup, returns masked data only — see PatientLookupResult.
    Task<PatientLookupResult> LookupByPhoneAsync(string phone, CancellationToken cancellationToken = default);

    Task<PatientDto> CreateAsync(CreatePatientRequest request, CancellationToken cancellationToken = default);

    // VC-09: reception creating a walk-in/phone patient with just a name and phone.
    Task<PatientDto> QuickCreateAsync(QuickCreatePatientRequest request, CancellationToken cancellationToken = default);

    // VC-02: on self-registration, link the new account to a pre-existing walk-in
    // profile with the same phone (so appointment history isn't lost), or create
    // a fresh one if none exists.
    Task<PatientDto> LinkOrCreateAsync(int userId, string fullName, string phone, DateOnly? dateOfBirth, CancellationToken cancellationToken = default);

    Task<PatientDto> UpdateAsync(int id, UpdatePatientRequest request, CancellationToken cancellationToken = default);
}

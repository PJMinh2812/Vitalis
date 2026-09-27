namespace Vitalis.Application.DTOs.Patients;

// VC-02: called before the caller is authenticated (self-registration flow), so it
// must never carry more than enough for someone to recognize their own old
// walk-in profile — never phone/address/national id, which belong to someone
// who might not be the caller.
public record PatientLookupResult(bool Found, string? PatientCode, string? MaskedName);

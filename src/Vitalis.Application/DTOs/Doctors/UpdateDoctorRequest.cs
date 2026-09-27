namespace Vitalis.Application.DTOs.Doctors;

// Profile fields only — the login account itself is managed through
// UsersController (reset-password / lock), not here.
public record UpdateDoctorRequest(
    string FullName,
    string? Title,
    int SpecialtyId,
    string? LicenseNumber,
    string? Room,
    decimal ConsultationFee,
    int? MaxPatientsPerDay);

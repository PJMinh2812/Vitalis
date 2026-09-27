namespace Vitalis.Application.DTOs.Doctors;

// Creates the login account (auth.users + Doctor role) and the doctor profile
// together — Username/Password are required here even though VC-19 shows them
// as optional fields, because a doctor needs an account from day one and this
// project has no separate "create account later" flow. See learning-notes.
public record DoctorRequest(
    string FullName,
    string? Title,
    int SpecialtyId,
    string? LicenseNumber,
    string? Room,
    decimal ConsultationFee,
    int? MaxPatientsPerDay,
    string Username,
    string Password);

namespace Vitalis.Application.DTOs.Doctors;

public record DoctorDto(
    int Id,
    string FullName,
    string? Title,
    int SpecialtyId,
    string SpecialtyName,
    string? LicenseNumber,
    string? Room,
    decimal? ConsultationFee,
    int? MaxPatientsPerDay,
    bool IsActive,
    int UserId,
    string Username);

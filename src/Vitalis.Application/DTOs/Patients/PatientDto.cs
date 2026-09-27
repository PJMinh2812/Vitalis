using Vitalis.Domain.Enums;

namespace Vitalis.Application.DTOs.Patients;

public record PatientDto(
    int Id,
    string? PatientCode,
    string FullName,
    Gender? Gender,
    DateOnly? DateOfBirth,
    string? Phone,
    string? Email,
    string? Address,
    string? NationalId,
    string? InsuranceNumber,
    string? BloodType,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    bool HasAccount,
    bool IsActive,
    DateTime CreatedAt);

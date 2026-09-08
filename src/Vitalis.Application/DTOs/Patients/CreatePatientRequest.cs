using Vitalis.Domain.Enums;

namespace Vitalis.Application.DTOs.Patients;

public record CreatePatientRequest
{
    public required string FullName { get; init; }
    public required string Phone { get; init; }
    public Gender? Gender { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public string? Email { get; init; }
    public string? Address { get; init; }
    public string? NationalId { get; init; }
    public string? InsuranceNumber { get; init; }
    public string? BloodType { get; init; }
}

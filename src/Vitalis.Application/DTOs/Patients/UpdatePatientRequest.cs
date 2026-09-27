using Vitalis.Domain.Enums;

namespace Vitalis.Application.DTOs.Patients;

// All fields are optional except FullName: a patient record can't be nameless,
// but every other field should keep its current value when omitted, not get
// wiped out by a partial edit.
public record UpdatePatientRequest
{
    public required string FullName { get; init; }
    public string? Phone { get; init; }
    public Gender? Gender { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public string? Email { get; init; }
    public string? Address { get; init; }
    public string? NationalId { get; init; }
    public string? InsuranceNumber { get; init; }
    public string? BloodType { get; init; }
}

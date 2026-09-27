namespace Vitalis.Application.DTOs.Patients;

// VC-09: reception booking on behalf of a walk-in/phone patient with no account yet.
public record QuickCreatePatientRequest
{
    public required string FullName { get; init; }
    public required string Phone { get; init; }
}

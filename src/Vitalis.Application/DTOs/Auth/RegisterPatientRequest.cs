namespace Vitalis.Application.DTOs.Auth;

public record RegisterPatientRequest
{
    public required string FullName { get; init; }
    public required string Phone { get; init; }
    public string? Email { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public required string Password { get; init; }
}

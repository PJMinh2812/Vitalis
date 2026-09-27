namespace Vitalis.Application.DTOs.Auth;

public record UserSummaryDto(int Id, string FullName, IReadOnlyList<string> Roles, int? DoctorId, int? PatientId);

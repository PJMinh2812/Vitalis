using Vitalis.Domain.Entities.Scheduling;

namespace Vitalis.Application.DTOs.Doctors;

// No EF Core Include in Application (deliberate) — specialtyName/username are
// resolved by the caller (batch-loaded dictionaries in DoctorService) and
// passed in, same in-memory-join pattern as UserService.
public static class DoctorMappingExtensions
{
    public static DoctorDto ToDto(this Doctor doctor, string specialtyName, string username) => new(
        doctor.Id,
        doctor.FullName,
        doctor.Title,
        doctor.SpecialtyId,
        specialtyName,
        doctor.LicenseNumber,
        doctor.Room,
        doctor.ConsultationFee,
        doctor.MaxPatientsPerDay,
        doctor.IsActive,
        doctor.UserId,
        username);
}

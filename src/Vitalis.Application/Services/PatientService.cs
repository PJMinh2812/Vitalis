using Vitalis.Application.Common;
using Vitalis.Application.DTOs.Patients;
using Vitalis.Application.Exceptions;
using Vitalis.Application.Interfaces;
using Vitalis.Domain.Entities.Scheduling;

namespace Vitalis.Application.Services;

public class PatientService(IUnitOfWork unitOfWork) : IPatientService
{
    public async Task<PatientDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var patient = await unitOfWork.Repository<Patient>().GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Patient), id);

        return patient.ToDto();
    }

    public async Task<PagedResult<PatientDto>> SearchAsync(string? keyword, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var repository = unitOfWork.Repository<Patient>();
        var query = repository.Query();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            // StartsWith translates to "LIKE 'value%'", which IX_patients_full_name and
            // IX_patients_phone can actually use — Contains ("LIKE '%value%'") cannot.
            query = query.Where(p =>
                p.FullName.StartsWith(keyword) ||
                (p.Phone != null && p.Phone.StartsWith(keyword)) ||
                p.PatientCode == keyword);
        }

        query = query.OrderBy(p => p.FullName);

        var paged = await repository.GetPagedAsync(query, page, pageSize, cancellationToken);

        return new PagedResult<PatientDto>(
            paged.Items.Select(p => p.ToDto()).ToList(),
            paged.TotalCount,
            paged.Page,
            paged.PageSize);
    }

    public async Task<PatientLookupResult> LookupByPhoneAsync(string phone, CancellationToken cancellationToken = default)
    {
        var repository = unitOfWork.Repository<Patient>();
        var patient = await repository.FirstOrDefaultAsync(repository.Query().Where(p => p.Phone == phone), cancellationToken);

        return patient is null
            ? new PatientLookupResult(false, null, null)
            : new PatientLookupResult(true, patient.PatientCode, MaskName(patient.FullName));
    }

    public Task<PatientDto> CreateAsync(CreatePatientRequest request, CancellationToken cancellationToken = default) =>
        SaveNewPatientAsync(new Patient
        {
            FullName = request.FullName,
            Phone = request.Phone,
            Gender = request.Gender,
            DateOfBirth = request.DateOfBirth,
            Email = request.Email,
            Address = request.Address,
            NationalId = request.NationalId,
            InsuranceNumber = request.InsuranceNumber,
            BloodType = request.BloodType,
        }, cancellationToken);

    public Task<PatientDto> QuickCreateAsync(QuickCreatePatientRequest request, CancellationToken cancellationToken = default) =>
        SaveNewPatientAsync(new Patient
        {
            FullName = request.FullName,
            Phone = request.Phone,
        }, cancellationToken);

    public async Task<PatientDto> UpdateAsync(int id, UpdatePatientRequest request, CancellationToken cancellationToken = default)
    {
        var repository = unitOfWork.Repository<Patient>();
        var patient = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Patient), id);

        patient.FullName = request.FullName;
        if (request.Phone is not null) patient.Phone = request.Phone;
        if (request.Gender is not null) patient.Gender = request.Gender;
        if (request.DateOfBirth is not null) patient.DateOfBirth = request.DateOfBirth;
        if (request.Email is not null) patient.Email = request.Email;
        if (request.Address is not null) patient.Address = request.Address;
        if (request.NationalId is not null) patient.NationalId = request.NationalId;
        if (request.InsuranceNumber is not null) patient.InsuranceNumber = request.InsuranceNumber;
        if (request.BloodType is not null) patient.BloodType = request.BloodType;
        patient.UpdatedAt = DateTime.UtcNow;

        repository.Update(patient);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return patient.ToDto();
    }

    public async Task<PatientDto> LinkOrCreateAsync(int userId, string fullName, string phone, DateOnly? dateOfBirth, CancellationToken cancellationToken = default)
    {
        var repository = unitOfWork.Repository<Patient>();
        var existing = await repository.FirstOrDefaultAsync(
            repository.Query().Where(p => p.Phone == phone && p.UserId == null), cancellationToken);

        if (existing is not null)
        {
            existing.UserId = userId;
            existing.UpdatedAt = DateTime.UtcNow;
            repository.Update(existing);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return existing.ToDto();
        }

        return await SaveNewPatientAsync(new Patient
        {
            UserId = userId,
            FullName = fullName,
            Phone = phone,
            DateOfBirth = dateOfBirth,
        }, cancellationToken);
    }

    // Two round trips, on purpose: the code is derived from the IDENTITY value SQL
    // Server assigns on insert, so it can never collide even if two patients are
    // created at the exact same time (see the AppointmentStatus/slot discussion for
    // why a COUNT/MAX-based code would be a race condition).
    private async Task<PatientDto> SaveNewPatientAsync(Patient patient, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.Repository<Patient>();
        await repository.AddAsync(patient, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        patient.PatientCode = $"BN{patient.Id:D4}";
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return patient.ToDto();
    }

    // "Nguyễn Văn An" -> "Nguyễn V*** A": keep the surname, reduce every middle
    // token to its initial, and the given name to just its initial.
    private static string MaskName(string fullName)
    {
        var tokens = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length <= 1)
            return fullName.Length > 0 ? $"{fullName[..1]}***" : fullName;

        var first = tokens[0];
        var last = tokens[^1][..1];

        if (tokens.Length == 2)
            return $"{first} {last}";

        var middle = string.Join(' ', tokens.Skip(1).Take(tokens.Length - 2).Select(t => $"{t[..1]}***"));
        return $"{first} {middle} {last}";
    }
}

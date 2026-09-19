using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vitalis.Application.Common;
using Vitalis.Application.DTOs.Billing;
using Vitalis.Application.DTOs.MedicalRecords;
using Vitalis.Application.Exceptions;
using Vitalis.Application.Interfaces;

namespace Vitalis.WebApi.Controllers;

[ApiController]
[Route("api/medical-records")]
[Authorize]
public class MedicalRecordsController(IMedicalRecordsService medicalRecordsService) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult<MedicalRecordDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await medicalRecordsService.GetByIdAsync(id, cancellationToken);
        EnsureCanAccess(result);
        return Ok(result);
    }

    // VC-05: patient's own finalized visit history.
    [HttpGet("/api/patients/me/records")]
    [Authorize(Roles = "Patient")]
    public async Task<ActionResult<PagedResult<MedicalRecordDto>>> GetMyRecords(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await medicalRecordsService.GetPatientHistoryAsync(GetOwnPatientId(), page, pageSize, cancellationToken);
        return Ok(result);
    }

    // VC-13 #1 — "Lưu nháp"
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Doctor")]
    public async Task<ActionResult<MedicalRecordDto>> SaveDraft(int id, MedicalRecordRequest request, CancellationToken cancellationToken)
    {
        EnsureCanAccess(await medicalRecordsService.GetByIdAsync(id, cancellationToken));
        var result = await medicalRecordsService.SaveDraftAsync(id, request, cancellationToken);
        return Ok(result);
    }

    // VC-14 #1
    [HttpPost("{id:int}/services")]
    [Authorize(Roles = "Doctor")]
    public async Task<ActionResult<MedicalRecordServiceDto>> OrderService(int id, OrderServiceRequest request, CancellationToken cancellationToken)
    {
        EnsureCanAccess(await medicalRecordsService.GetByIdAsync(id, cancellationToken));
        var result = await medicalRecordsService.OrderServiceAsync(id, request, cancellationToken);
        return Ok(result);
    }

    // VC-15 #2
    [HttpPut("{id:int}/prescription")]
    [Authorize(Roles = "Doctor")]
    public async Task<ActionResult<PrescriptionDto>> SavePrescription(int id, PrescriptionRequest request, CancellationToken cancellationToken)
    {
        EnsureCanAccess(await medicalRecordsService.GetByIdAsync(id, cancellationToken));
        var result = await medicalRecordsService.SavePrescriptionAsync(id, request, GetOwnDoctorId(), cancellationToken);
        return Ok(result);
    }

    // VC-13 #4 — point of no return.
    [HttpPost("{id:int}/finalize")]
    [Authorize(Roles = "Doctor")]
    public async Task<ActionResult<InvoiceDto>> Finalize(int id, CancellationToken cancellationToken)
    {
        EnsureCanAccess(await medicalRecordsService.GetByIdAsync(id, cancellationToken));
        var result = await medicalRecordsService.FinalizeAsync(id, cancellationToken);
        return Ok(result);
    }

    private void EnsureCanAccess(MedicalRecordDto record)
    {
        if (User.IsInRole("Patient") && record.PatientId != GetOwnPatientId())
            throw new ForbiddenException("Không có quyền truy cập bệnh án này");

        if (User.IsInRole("Doctor") && record.DoctorId != GetOwnDoctorId())
            throw new ForbiddenException("Không có quyền truy cập bệnh án này");
    }

    private int GetOwnPatientId() =>
        int.TryParse(User.FindFirstValue("patientId"), out var id) ? id : throw new ForbiddenException("Tài khoản chưa liên kết hồ sơ bệnh nhân");

    private int GetOwnDoctorId() =>
        int.TryParse(User.FindFirstValue("doctorId"), out var id) ? id : throw new ForbiddenException("Tài khoản chưa liên kết hồ sơ bác sĩ");
}

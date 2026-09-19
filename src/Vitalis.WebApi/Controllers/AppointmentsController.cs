using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vitalis.Application.Common;
using Vitalis.Application.DTOs.Appointments;
using Vitalis.Application.Exceptions;
using Vitalis.Application.Interfaces;
using Vitalis.Domain.Enums;

namespace Vitalis.WebApi.Controllers;

// Every account in this project holds exactly one primary role (see DbSeeder),
// so a simple if/else-if on role is enough — no multi-role priority policy needed.
[ApiController]
[Route("api/appointments")]
[Authorize]
public class AppointmentsController(IAppointmentService appointmentService, IMedicalRecordsService medicalRecordsService) : ControllerBase
{
    // POST /api/appointments   (VC-03 patient self-book, VC-09 reception on-behalf)
    [HttpPost]
    [Authorize(Roles = "Patient,Receptionist,Admin")]
    public async Task<ActionResult<AppointmentDto>> Create(CreateAppointmentRequest request, CancellationToken cancellationToken)
    {
        if (User.IsInRole("Patient") && request.PatientId != GetOwnPatientId())
            throw new ForbiddenException("Không thể đặt lịch hộ người khác");

        var result = await appointmentService.BookAsync(request, GetUserId(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // GET /api/appointments?patientId=&doctorId=&date=&status=&page=&pageSize=
    [HttpGet]
    public async Task<ActionResult<PagedResult<AppointmentDto>>> Search(
        [FromQuery] int? patientId,
        [FromQuery] int? doctorId,
        [FromQuery] DateOnly? date,
        [FromQuery] AppointmentStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (User.IsInRole("Patient"))
            patientId = GetOwnPatientId();
        else if (User.IsInRole("Doctor"))
            doctorId = GetOwnDoctorId();

        var result = await appointmentService.SearchAsync(patientId, doctorId, date, status, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AppointmentDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await appointmentService.GetByIdAsync(id, cancellationToken);
        EnsureCanAccess(result);
        return Ok(result);
    }

    // PATCH /api/appointments/{id}/cancel   (VC-04)
    [HttpPatch("{id:int}/cancel")]
    public async Task<ActionResult<AppointmentDto>> Cancel(int id, CancelAppointmentRequest request, CancellationToken cancellationToken)
    {
        EnsureCanAccess(await appointmentService.GetByIdAsync(id, cancellationToken));
        var result = await appointmentService.CancelAsync(id, request.CancelReason, GetUserId(), cancellationToken);
        return Ok(result);
    }

    // PATCH /api/appointments/{id}/reschedule   (VC-04)
    [HttpPatch("{id:int}/reschedule")]
    public async Task<ActionResult<AppointmentDto>> Reschedule(int id, RescheduleAppointmentRequest request, CancellationToken cancellationToken)
    {
        EnsureCanAccess(await appointmentService.GetByIdAsync(id, cancellationToken));
        var result = await appointmentService.RescheduleAsync(id, request.NewStartTime, GetUserId(), cancellationToken);
        return Ok(result);
    }

    // PATCH /api/appointments/{id}/check-in   (VC-07, reception only)
    [HttpPatch("{id:int}/check-in")]
    [Authorize(Roles = "Receptionist,Admin")]
    public async Task<ActionResult<AppointmentDto>> CheckIn(int id, CancellationToken cancellationToken)
    {
        var result = await appointmentService.CheckInAsync(id, GetUserId(), cancellationToken);
        return Ok(result);
    }

    // PATCH /api/appointments/{id}/no-show   (VC-07, reception only)
    [HttpPatch("{id:int}/no-show")]
    [Authorize(Roles = "Receptionist,Admin")]
    public async Task<ActionResult<AppointmentDto>> NoShow(int id, NoShowAppointmentRequest request, CancellationToken cancellationToken)
    {
        var result = await appointmentService.MarkNoShowAsync(id, request.Note, GetUserId(), cancellationToken);
        return Ok(result);
    }

    // PATCH /api/appointments/{id}/start-exam   (VC-12/VC-13 doctor queue -> exam
    // screen) — bridges Appointment (CheckedIn -> InProgress) into MedicalRecords
    // (creates the Draft record), a Doctor-only action.
    [HttpPatch("{id:int}/start-exam")]
    [Authorize(Roles = "Doctor")]
    public async Task<ActionResult<Application.DTOs.MedicalRecords.MedicalRecordDto>> StartExam(int id, CancellationToken cancellationToken)
    {
        var appointment = await appointmentService.GetByIdAsync(id, cancellationToken);
        if (appointment.DoctorId != GetOwnDoctorId())
            throw new ForbiddenException("Không có quyền bắt đầu khám lịch hẹn này");

        var result = await medicalRecordsService.StartExamAsync(id, cancellationToken);
        return Ok(result);
    }

    // A patient may only touch their own appointments; a doctor may only view
    // (not modify — cancel/reschedule are Patient/Receptionist/Admin actions
    // above) their own. Receptionist/Admin are unrestricted.
    private void EnsureCanAccess(AppointmentDto appointment)
    {
        if (User.IsInRole("Patient") && appointment.PatientId != GetOwnPatientId())
            throw new ForbiddenException("Không có quyền truy cập lịch hẹn này");

        if (User.IsInRole("Doctor") && appointment.DoctorId != GetOwnDoctorId())
            throw new ForbiddenException("Không có quyền truy cập lịch hẹn này");
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private int GetOwnPatientId() =>
        int.TryParse(User.FindFirstValue("patientId"), out var id) ? id : throw new ForbiddenException("Tài khoản chưa liên kết hồ sơ bệnh nhân");

    private int GetOwnDoctorId() =>
        int.TryParse(User.FindFirstValue("doctorId"), out var id) ? id : throw new ForbiddenException("Tài khoản chưa liên kết hồ sơ bác sĩ");
}

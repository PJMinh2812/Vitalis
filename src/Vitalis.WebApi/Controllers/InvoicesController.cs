using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vitalis.Application.DTOs.Billing;
using Vitalis.Application.Exceptions;
using Vitalis.Application.Interfaces;

namespace Vitalis.WebApi.Controllers;

[ApiController]
[Authorize]
public class InvoicesController(IInvoiceService invoiceService) : ControllerBase
{
    [HttpGet("api/invoices/{id:int}")]
    public async Task<ActionResult<InvoiceDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await invoiceService.GetByIdAsync(id, cancellationToken);
        if (User.IsInRole("Patient") && result.PatientId != GetOwnPatientId())
            throw new ForbiddenException("Không có quyền truy cập hoá đơn này");

        return Ok(result);
    }

    // VC-06
    [HttpGet("/api/patients/me/invoices")]
    [Authorize(Roles = "Patient")]
    public async Task<ActionResult<IReadOnlyList<InvoiceDto>>> GetMyInvoices(CancellationToken cancellationToken)
    {
        var result = await invoiceService.GetPatientInvoicesAsync(GetOwnPatientId(), cancellationToken);
        return Ok(result);
    }

    // VC-11 #1 (thu tiền) and #3 (hoàn tiền — IsRefund = true, needs Admin: "cần quyền riêng").
    [HttpPost("api/invoices/{id:int}/payments")]
    [Authorize(Roles = "Receptionist,Admin")]
    public async Task<ActionResult<InvoiceDto>> AddPayment(int id, PaymentRequest request, CancellationToken cancellationToken)
    {
        if (request.IsRefund && !User.IsInRole("Admin"))
            throw new ForbiddenException("Hoàn tiền cần quyền Admin");

        var receivedBy = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await invoiceService.AddPaymentAsync(id, request, receivedBy, cancellationToken);
        return Ok(result);
    }

    // VC-11 #4 — only while paid_amount == 0.
    [HttpPatch("api/invoices/{id:int}/cancel")]
    [Authorize(Roles = "Receptionist,Admin")]
    public async Task<ActionResult<InvoiceDto>> Cancel(int id, CancelInvoiceRequest request, CancellationToken cancellationToken)
    {
        var result = await invoiceService.CancelAsync(id, request.CancelReason, cancellationToken);
        return Ok(result);
    }

    private int GetOwnPatientId() =>
        int.TryParse(User.FindFirstValue("patientId"), out var id) ? id : throw new ForbiddenException("Tài khoản chưa liên kết hồ sơ bệnh nhân");
}

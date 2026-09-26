using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vitalis.Application.DTOs.Medicines;
using Vitalis.Application.DTOs.Pharmacy;
using Vitalis.Application.Interfaces;

namespace Vitalis.WebApi.Controllers;

[ApiController]
[Authorize]
public class MedicinesController(IMedicineService medicineService, IPharmacyService pharmacyService) : ControllerBase
{
    // GET /api/medicines/search?keyword=&patientId=   (VC-15 #1)
    [HttpGet("api/medicines/search")]
    public async Task<ActionResult<IReadOnlyList<MedicineSearchDto>>> Search(
        [FromQuery] string? keyword, [FromQuery] int? patientId, CancellationToken cancellationToken)
    {
        var result = await medicineService.SearchAsync(keyword, patientId, cancellationToken);
        return Ok(result);
    }

    // POST /api/admin/medicines — minimal catalog create; per-lot stock (VC-17/18) is below.
    [HttpPost("api/admin/medicines")]
    [Authorize(Roles = "Admin,Pharmacist")]
    public async Task<ActionResult<MedicineDto>> Create(CreateMedicineRequest request, CancellationToken cancellationToken)
    {
        var result = await medicineService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Search), result);
    }

    // GET /api/medicines/inventory?lowStock=&nearExpiry=   (VC-17 #1)
    [HttpGet("api/medicines/inventory")]
    [Authorize(Roles = "Admin,Pharmacist")]
    public async Task<ActionResult<IReadOnlyList<MedicineInventoryDto>>> GetInventory(
        [FromQuery] bool? lowStock, [FromQuery] bool? nearExpiry, CancellationToken cancellationToken)
    {
        var result = await pharmacyService.GetInventoryAsync(lowStock, nearExpiry, cancellationToken);
        return Ok(result);
    }

    // POST /api/medicines/receipts   (VC-18 #2)
    [HttpPost("api/medicines/receipts")]
    [Authorize(Roles = "Admin,Pharmacist")]
    public async Task<ActionResult<IReadOnlyList<MedicineDto>>> CreateReceipt(MedicineReceiptRequest request, CancellationToken cancellationToken)
    {
        var receivedBy = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var result = await pharmacyService.CreateReceiptAsync(request, receivedBy, cancellationToken);
        return Ok(result);
    }

    // POST /api/medicines/{id}/adjust   (VC-17 #3)
    [HttpPost("api/medicines/{id:int}/adjust")]
    [Authorize(Roles = "Admin,Pharmacist")]
    public async Task<IActionResult> AdjustStock(int id, AdjustStockRequest request, CancellationToken cancellationToken)
    {
        var adjustedBy = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        await pharmacyService.AdjustStockAsync(id, request, adjustedBy, cancellationToken);
        return NoContent();
    }

    // GET /api/medicines/{id}/transactions?from=&to=   (VC-17 #4, "thẻ kho")
    [HttpGet("api/medicines/{id:int}/transactions")]
    [Authorize(Roles = "Admin,Pharmacist")]
    public async Task<ActionResult<IReadOnlyList<StockTransactionDto>>> GetTransactions(
        int id, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var result = await pharmacyService.GetTransactionsAsync(id, from, to, cancellationToken);
        return Ok(result);
    }
}

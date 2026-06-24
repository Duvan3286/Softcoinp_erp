using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Softcoinp.ERP.WebAPI.DTOs;
using Softcoinp.ERP.WebAPI.Services;

namespace Softcoinp.ERP.WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/reservation")]
public class ReservationController : BaseController
{
    private readonly ReservationService _reservationService;

    public ReservationController(ReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    // ── Reservable Spaces ────────────────────────────────────────

    [HttpGet("spaces")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<List<ReservableSpaceListDto>>> GetSpaces(
        [FromQuery] bool? isActive = null)
    {
        var tenantId = GetTenantId();
        var spaces = await _reservationService.GetSpacesAsync(tenantId, isActive);
        return Ok(spaces);
    }

    [HttpGet("spaces/{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<ReservableSpaceDetailDto>> GetSpace(Guid id)
    {
        var tenantId = GetTenantId();
        var space = await _reservationService.GetSpaceByIdAsync(id, tenantId);
        return Ok(space);
    }

    [HttpPost("spaces")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ReservableSpaceDetailDto>> CreateSpace(
        [FromBody] CreateReservableSpaceRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var space = await _reservationService.CreateSpaceAsync(request, tenantId, userId);
        return CreatedAtAction(nameof(GetSpace), new { id = space.Id }, space);
    }

    [HttpPut("spaces/{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ReservableSpaceDetailDto>> UpdateSpace(
        Guid id, [FromBody] UpdateReservableSpaceRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var space = await _reservationService.UpdateSpaceAsync(id, request, tenantId, userId);
        return Ok(space);
    }

    // ── Schedules ────────────────────────────────────────────────

    [HttpGet("spaces/{spaceId:guid}/schedules")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<List<SpaceScheduleDto>>> GetSchedules(Guid spaceId)
    {
        var tenantId = GetTenantId();
        var schedules = await _reservationService.GetSchedulesAsync(spaceId, tenantId);
        return Ok(schedules);
    }

    [HttpPost("spaces/{spaceId:guid}/schedules")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<SpaceScheduleDto>> CreateSchedule(
        Guid spaceId, [FromBody] CreateSpaceScheduleRequestDto request)
    {
        var tenantId = GetTenantId();
        var schedule = await _reservationService.CreateScheduleAsync(spaceId, request, tenantId);
        return Created("", schedule);
    }

    [HttpDelete("schedules/{scheduleId:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> DeleteSchedule(Guid scheduleId)
    {
        var tenantId = GetTenantId();
        await _reservationService.DeleteScheduleAsync(scheduleId, tenantId);
        return NoContent();
    }

    // ── Space Blocks ─────────────────────────────────────────────

    [HttpGet("blocks")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<List<SpaceBlockDto>>> GetBlocks(
        [FromQuery] Guid? spaceId = null)
    {
        var tenantId = GetTenantId();
        var blocks = await _reservationService.GetBlocksAsync(tenantId, spaceId);
        return Ok(blocks);
    }

    [HttpPost("blocks")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<SpaceBlockDto>> CreateBlock(
        [FromBody] CreateSpaceBlockRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var block = await _reservationService.CreateBlockAsync(request, tenantId, userId);
        return Created("", block);
    }

    // ── Reservations ─────────────────────────────────────────────

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<List<ReservationListDto>>> GetReservations(
        [FromQuery] string? status = null,
        [FromQuery] Guid? spaceId = null,
        [FromQuery] Guid? unitId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var tenantId = GetTenantId();
        var reservations = await _reservationService.GetReservationsAsync(
            tenantId, status, spaceId, unitId, fromDate, toDate);
        return Ok(reservations);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<ReservationDetailDto>> GetReservation(Guid id)
    {
        var tenantId = GetTenantId();
        var reservation = await _reservationService.GetReservationByIdAsync(id, tenantId);
        return Ok(reservation);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<ReservationDetailDto>> CreateReservation(
        [FromBody] CreateReservationRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var reservation = await _reservationService.CreateReservationAsync(request, tenantId, userId);
        return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> ApproveReservation(
        Guid id, [FromBody] ApproveReservationRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        await _reservationService.ApproveReservationAsync(id, request, tenantId, userId);
        return Ok(new { message = "Reserva aprobada exitosamente" });
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> RejectReservation(
        Guid id, [FromBody] RejectReservationRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        await _reservationService.RejectReservationAsync(id, request, tenantId, userId);
        return Ok(new { message = "Reserva rechazada exitosamente" });
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> CancelReservation(Guid id)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        await _reservationService.CancelReservationAsync(id, tenantId, userId);
        return Ok(new { message = "Reserva cancelada exitosamente" });
    }

    [HttpPost("{id:guid}/check-in")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> CheckIn(
        Guid id, [FromBody] CheckInReservationRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        await _reservationService.CheckInReservationAsync(id, request, tenantId, userId);
        return Ok(new { message = "Check-in registrado exitosamente" });
    }

    [HttpPost("{id:guid}/check-out")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> CheckOut(
        Guid id, [FromBody] CheckOutReservationRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        await _reservationService.CheckOutReservationAsync(id, request, tenantId, userId);
        return Ok(new { message = "Check-out registrado exitosamente" });
    }

    [HttpPost("{reservationId:guid}/incidents")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> ReportIncident(
        Guid reservationId, [FromBody] ReportIncidentRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        await _reservationService.ReportIncidentAsync(reservationId, request, tenantId, userId);
        return Ok(new { message = "Incidente registrado exitosamente" });
    }

    // ── Deposits ─────────────────────────────────────────────────

    [HttpPost("{reservationId:guid}/deposits/pay")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> ProcessDepositPayment(
        Guid reservationId, [FromBody] ProcessDepositPaymentRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        await _reservationService.ProcessDepositPaymentAsync(reservationId, request, tenantId, userId);
        return Ok(new { message = "Pago de depósito registrado exitosamente" });
    }

    [HttpPost("{reservationId:guid}/deposits/return")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> ProcessDepositReturn(
        Guid reservationId, [FromBody] ProcessDepositReturnRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        await _reservationService.ProcessDepositReturnAsync(reservationId, request, tenantId, userId);
        return Ok(new { message = "Devolución de depósito procesada exitosamente" });
    }

    [HttpPost("{reservationId:guid}/deposits/apply-damage")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> ApplyDepositToDamage(
        Guid reservationId, [FromBody] ApplyDepositToDamageRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        await _reservationService.ApplyDepositToDamageAsync(reservationId, request, tenantId, userId);
        return Ok(new { message = "Depósito aplicado a daño exitosamente" });
    }

    // ── Availability ─────────────────────────────────────────────

    [HttpGet("availability")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<AvailabilityCheckDto>> CheckAvailability(
        [FromQuery] Guid spaceId,
        [FromQuery] DateTime start,
        [FromQuery] DateTime end,
        [FromQuery] Guid unitId)
    {
        var tenantId = GetTenantId();
        var result = await _reservationService.CheckAvailabilityAsync(spaceId, start, end, unitId, tenantId);
        return Ok(result);
    }

    [HttpGet("availability/slots")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<List<AvailableSlotDto>>> GetAvailableSlots(
        [FromQuery] Guid spaceId,
        [FromQuery] DateTime date)
    {
        var tenantId = GetTenantId();
        var slots = await _reservationService.GetAvailableSlotsAsync(spaceId, date, tenantId);
        return Ok(slots);
    }

    [HttpGet("availability/alternatives")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<List<AlternativeSlotDto>>> GetAlternatives(
        [FromQuery] Guid spaceId,
        [FromQuery] DateTime start,
        [FromQuery] DateTime end)
    {
        var tenantId = GetTenantId();
        var alternatives = await _reservationService.GetAlternativesAsync(spaceId, start, end, tenantId);
        return Ok(alternatives);
    }

    // ── Calendar ─────────────────────────────────────────────────

    [HttpGet("calendar/{spaceId:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<List<CalendarEventDto>>> GetCalendarEvents(
        Guid spaceId,
        [FromQuery] DateTime monthStart,
        [FromQuery] DateTime monthEnd)
    {
        var tenantId = GetTenantId();
        var events = await _reservationService.GetCalendarEventsAsync(spaceId, monthStart, monthEnd, tenantId);
        return Ok(events);
    }

    // ── Reports ──────────────────────────────────────────────────

    [HttpGet("reports/{spaceId:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<ReservationReportDto>> GetReport(
        Guid spaceId,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        var tenantId = GetTenantId();
        var report = await _reservationService.GetReportAsync(spaceId, fromDate, toDate, tenantId);
        return Ok(report);
    }
}

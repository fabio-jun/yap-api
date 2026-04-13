using System.Security.Claims;
using Blog.Application.DTOs.Reports;
using Blog.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Blog.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create report", Description = "Reports a user, a yap, or both for moderator review.")]
    public async Task<IActionResult> Create(CreateReportRequest request)
    {
        var reporterId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var report = await _reportService.CreateAsync(request, reporterId);
        return Ok(report);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Get all reports", Description = "Admin-only endpoint that returns all moderation reports.")]
    public async Task<IActionResult> GetAll()
    {
        var reports = await _reportService.GetAllAsync();
        return Ok(reports);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Update report status", Description = "Admin-only endpoint that marks a report as pending, reviewed, or dismissed.")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateReportStatusRequest request)
    {
        var reviewerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var report = await _reportService.UpdateStatusAsync(id, request.Status, reviewerId);
        return Ok(report);
    }
}

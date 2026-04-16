using Blog.Application.DTOs.Reports;
using Blog.Application.Interfaces;
using Blog.Domain.Entities;
using Blog.Domain.Interfaces;

namespace Blog.Application.Services;

// Service that handles moderation reports and admin review status changes.
public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPostRepository _postRepository;

    // Repositories are injected by ASP.NET Core's DI container.
    public ReportService(
        IReportRepository reportRepository,
        IUserRepository userRepository,
        IPostRepository postRepository)
    {
        _reportRepository = reportRepository;
        _userRepository = userRepository;
        _postRepository = postRepository;
    }

    // Creates a pending report for a user, a yap, or both.
    public async Task<ReportResponse> CreateAsync(CreateReportRequest request, int reporterId)
    {
        // A report must point at something actionable for moderation.
        if (!request.ReportedUserId.HasValue && !request.PostId.HasValue)
            throw new ArgumentException("A report must target a user or a yap.");

        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new ArgumentException("Report reason is required.");

        if (request.ReportedUserId == reporterId)
            throw new ArgumentException("You cannot report yourself.");

        // Validate references before persisting the report.
        if (request.ReportedUserId.HasValue && await _userRepository.GetByIdAsync(request.ReportedUserId.Value) == null)
            throw new KeyNotFoundException("Reported user not found.");

        if (request.PostId.HasValue && await _postRepository.GetByIdAsync(request.PostId.Value) == null)
            throw new KeyNotFoundException("Reported yap not found.");

        var report = new Report
        {
            ReporterId = reporterId,
            ReportedUserId = request.ReportedUserId,
            PostId = request.PostId,
            Reason = request.Reason.Trim(),
            Details = string.IsNullOrWhiteSpace(request.Details) ? null : request.Details.Trim(),
            Status = ReportStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _reportRepository.AddAsync(report);
        return Map(report);
    }

    // Returns all reports for the admin dashboard.
    public async Task<List<ReportResponse>> GetAllAsync()
    {
        var reports = await _reportRepository.GetAllAsync();
        return reports.Select(Map).ToList();
    }

    // Updates moderation status and stores who reviewed the report.
    public async Task<ReportResponse> UpdateStatusAsync(int id, string status, int reviewerId)
    {
        var report = await _reportRepository.GetByIdAsync(id);
        if (report == null)
            throw new KeyNotFoundException("Report not found.");

        // Accepts case-insensitive values that match the ReportStatus enum.
        if (!Enum.TryParse<ReportStatus>(status, ignoreCase: true, out var parsed))
            throw new ArgumentException("Invalid report status.");

        report.Status = parsed;
        report.ReviewerId = reviewerId;
        report.ReviewedAt = DateTime.UtcNow;
        await _reportRepository.UpdateAsync(report);

        return Map(report);
    }

    // Maps report entity data and optional navigation properties to the response DTO.
    private static ReportResponse Map(Report report)
    {
        return new ReportResponse
        {
            Id = report.Id,
            ReporterId = report.ReporterId,
            ReporterName = report.Reporter?.UserName ?? string.Empty,
            ReportedUserId = report.ReportedUserId,
            ReportedUserName = report.ReportedUser?.UserName,
            PostId = report.PostId,
            Reason = report.Reason,
            Details = report.Details,
            Status = report.Status.ToString().ToLower(),
            CreatedAt = report.CreatedAt,
            ReviewedAt = report.ReviewedAt,
            ReviewerId = report.ReviewerId,
            ReviewerName = report.Reviewer?.UserName
        };
    }
}

using Blog.Application.DTOs.Reports;

namespace Blog.Application.Interfaces;

public interface IReportService
{
    Task<ReportResponse> CreateAsync(CreateReportRequest request, int reporterId);
    Task<List<ReportResponse>> GetAllAsync();
    Task<ReportResponse> UpdateStatusAsync(int id, string status, int reviewerId);
}

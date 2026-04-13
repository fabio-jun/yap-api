namespace Blog.Application.DTOs.Reports;

public class CreateReportRequest
{
    public int? ReportedUserId { get; set; }
    public int? PostId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Details { get; set; }
}

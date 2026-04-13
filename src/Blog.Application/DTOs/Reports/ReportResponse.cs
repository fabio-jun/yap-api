namespace Blog.Application.DTOs.Reports;

public class ReportResponse
{
    public int Id { get; set; }
    public int ReporterId { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public int? ReportedUserId { get; set; }
    public string? ReportedUserName { get; set; }
    public int? PostId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public int? ReviewerId { get; set; }
    public string? ReviewerName { get; set; }
}

namespace Blog.Domain.Entities;

public enum ReportStatus
{
    Pending,
    Reviewed,
    Dismissed
}

public class Report
{
    // PK
    public int Id { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Details { get; set; }
    public ReportStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }

    public int ReporterId { get; set; }
    public User? Reporter { get; set; }

    public int? ReportedUserId { get; set; }
    public User? ReportedUser { get; set; }

    public int? PostId { get; set; }
    public Post? Post { get; set; }

    public int? ReviewerId { get; set; }
    public User? Reviewer { get; set; }
}

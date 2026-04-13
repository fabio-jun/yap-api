namespace Blog.Application.DTOs.Reposts;

public class RepostStateResponse
{
    public bool Reposted { get; set; }
    public int Count { get; set; }
    public int? RepostId { get; set; }
    public string? QuoteContent { get; set; }
}

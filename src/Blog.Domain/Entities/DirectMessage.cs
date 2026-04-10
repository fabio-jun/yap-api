namespace Blog.Domain.Entities;

public class DirectMessage
{
    public int Id { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public int SenderId { get; set; }
    public User? Sender { get; set; }
    public int ReceiverId { get; set; }
    public User? Receiver { get; set; }
}

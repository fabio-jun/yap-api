namespace Blog.Domain.Entities;

// Entity class — represents the "DirectMessages" table in the database.
// Models a private message from one user to another.
// Has two FK relationships to User: Sender and Receiver (configured with OnDelete Restrict
// to avoid circular cascade delete).
public class DirectMessage
{
    // Primary Key — auto-incremented
    public int Id { get; set; }

    // Message text content (max 2000 chars, validated in the service layer)
    public required string Content { get; set; }

    public DateTime CreatedAt { get; set; }

    // FK — the user who sent the message
    public int SenderId { get; set; }
    public User? Sender { get; set; }

    // FK — the user who receives the message
    public int ReceiverId { get; set; }
    public User? Receiver { get; set; }
}

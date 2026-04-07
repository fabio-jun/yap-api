namespace Blog.Application.DTOs.Tags;

// DTO for returning tag data to the client
public class TagResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int PostCount { get; set; }
}
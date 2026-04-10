namespace Blog.Application.DTOs.Tags;

// DTO for returning tag data to the client.
// Includes PostCount which is computed from the PostTags navigation property.
public class TagResponse
{
    public int Id { get; set; }

    // Tag name without '#' (e.g., "dev", "react")
    public required string Name { get; set; }

    // Number of posts using this tag — computed via tag.PostTags.Count in TagService
    public int PostCount { get; set; }
}

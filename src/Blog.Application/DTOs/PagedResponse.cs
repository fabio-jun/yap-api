namespace Blog.Application.DTOs;

// Generic DTO for paginated responses.
// <T> is a generic type parameter — the caller specifies what type of items the page contains.
// Example: PagedResponse<PostResponse> → a page of post responses.
public class PagedResponse<T>
{
    // The items on the current page. '= []' is C# 12 collection expression (same as new List<T>())
    public IEnumerable<T> Items { get; set; } = [];

    // Current page number (1-based)
    public int Page { get; set; }

    // Number of items per page
    public int PageSize { get; set; }

    // Total items across all pages (from COUNT query)
    public int TotalCount { get; set; }

    // Computed property (no setter) — calculates total pages using ceiling division.
    // '=>' is expression-bodied syntax: shorthand for { get { return ...; } }
    // Math.Ceiling rounds up: 51 items / 20 per page = 2.55 → 3 pages
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

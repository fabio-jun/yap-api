using Blog.Domain.Entities;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure;

// Static class that seeds the database with fake data using the Bogus library.
// static class — cannot be instantiated; all members must be static.
// Runs once on application startup; skips if data already exists.
public static class FakeDataSeeder
{
    // static method — called without creating an instance: FakeDataSeeder.SeedAsync(db)
    // async Task — asynchronous method that returns no value (Task is the async equivalent of void).
    public static async Task SeedAsync(AppDbContext db)
    {
        // Guard clause: if any users exist, the DB is already seeded — skip.
        // AnyAsync() translates to SQL: SELECT EXISTS(SELECT 1 FROM "Users")
        if (await db.Users.AnyAsync()) return;

        var now = DateTime.UtcNow;
        // Fixed seed (42) ensures reproducible fake data across runs — same seed = same data.
        Randomizer.Seed = new Random(42);

        // --- Users ---
        // Faker<User> — Bogus generic faker that creates User instances with rules.
        // RuleFor — defines how each property is populated.
        // f = Faker instance (provides random data generators), u = the User being built.
        var userFaker = new Faker<User>()
            .RuleFor(u => u.UserName, f => f.Internet.UserName().ToLower().Replace(".", "_"))
            // (f, u) — second overload: f is Faker, u is the partially built User (uses u.UserName set above).
            .RuleFor(u => u.Email, (f, u) => $"{u.UserName}@{f.Internet.DomainName()}")
            // _ — discard parameter (unused Faker instance). All users get the same password for testing.
            .RuleFor(u => u.PasswordHash, _ => BCrypt.Net.BCrypt.HashPassword("123456"))
            .RuleFor(u => u.Role, _ => "User")
            // PickRandom — randomly selects one value from the provided options.
            // null is a valid option — some users will have no bio.
            .RuleFor(u => u.Bio, f => f.PickRandom(
                f.Person.Company.Name + " | " + f.Hacker.IngVerb() + " stuff",
                f.Lorem.Sentence(4, 3),
                null
            ))
            .RuleFor(u => u.CreatedAt, f => now.AddDays(-f.Random.Int(30, 90)));

        // Generate(10) — creates 10 User instances using the rules above.
        var users = userFaker.Generate(10);
        // AddRange — stages multiple entities for INSERT at once (batch operation).
        db.Users.AddRange(users);
        // SaveChangesAsync — executes the INSERT SQL. After this, each user has an auto-generated Id.
        await db.SaveChangesAsync();

        // --- Follows ---
        var faker = new Faker();
        var follows = new List<Follow>();
        // foreach — iterates over each user to create follow relationships.
        foreach (var user in users)
        {
            // Select random users to follow (excluding self).
            // OrderBy(_ => faker.Random.Int()) — random shuffle.
            // Take(2..5) — each user follows 2-5 other users.
            var toFollow = users
                .Where(u => u.Id != user.Id)
                .OrderBy(_ => faker.Random.Int())
                .Take(faker.Random.Int(2, 5));

            foreach (var target in toFollow)
            {
                // Object initializer syntax — sets properties inline during construction.
                follows.Add(new Follow
                {
                    FollowerId = user.Id,
                    FollowingId = target.Id,
                    CreatedAt = now.AddDays(-faker.Random.Int(1, 30)),
                });
            }
        }

        // db.Set<Follow>() — accesses the DbSet dynamically by type (alternative to db.Follows if no property exists).
        db.Set<Follow>().AddRange(follows);
        await db.SaveChangesAsync();

        // --- Tags ---
        // string[] — array of predefined tag names.
        var tagNames = new[] { "dev", "dotnet", "react", "music", "gaming", "food", "travel", "tech", "memes", "fitness" };
        // .Select(t => new Tag { Name = t }) — LINQ projection: transforms each string into a Tag entity.
        // .ToList() — materializes the IEnumerable into a List<Tag>.
        var tags = tagNames.Select(t => new Tag { Name = t }).ToList();
        db.Set<Tag>().AddRange(tags);
        await db.SaveChangesAsync();

        // --- Posts (Yaps) ---
        var yapFaker = new Faker();
        var posts = new List<Post>();

        for (var i = 0; i < 50; i++)
        {
            var sentence = yapFaker.Lorem.Sentence(yapFaker.Random.Int(3, 12));
            // PickRandom(collection, count) — selects random tags to use as hashtags.
            var numTags = yapFaker.Random.Int(0, 3);
            var postTags = yapFaker.PickRandom(tagNames, numTags);
            // string.Join — concatenates the tag names with space separator, prefixing each with #.
            var hashtags = string.Join(" ", postTags.Select(t => $"#{t}"));
            var content = string.IsNullOrEmpty(hashtags) ? sentence : $"{sentence} {hashtags}";

            // content[..280] — range operator: equivalent to content.Substring(0, 280).
            // Enforces the 280-character limit for yaps.
            if (content.Length > 280)
                content = content[..280];

            var author = yapFaker.PickRandom(users);
            posts.Add(new Post
            {
                Content = content,
                AuthorId = author.Id,
                CreatedAt = now.AddHours(-yapFaker.Random.Int(1, 720)),
            });
        }

        db.Posts.AddRange(posts);
        await db.SaveChangesAsync();

        // --- PostTags ---
        // Parse hashtags from post content and create PostTag join entries.
        var postTagEntries = new List<PostTag>();
        foreach (var post in posts)
        {
            // Regex.Matches — finds all occurrences of #word in the content.
            // @"#(\w+)" — raw string literal (@), captures the word after # in group 1.
            var hashtagMatches = System.Text.RegularExpressions.Regex.Matches(post.Content, @"#(\w+)");
            // System.Text.RegularExpressions.Match — fully qualified type (no using directive at top).
            foreach (System.Text.RegularExpressions.Match match in hashtagMatches)
            {
                // match.Groups[1].Value — the captured group (tag name without the # symbol).
                // StringComparison.OrdinalIgnoreCase — case-insensitive comparison.
                var tag = tags.FirstOrDefault(t => t.Name.Equals(match.Groups[1].Value, StringComparison.OrdinalIgnoreCase));
                if (tag != null)
                {
                    postTagEntries.Add(new PostTag { PostId = post.Id, TagId = tag.Id });
                }
            }
        }

        db.Set<PostTag>().AddRange(postTagEntries);
        await db.SaveChangesAsync();

        // --- Likes ---
        // Each post gets 0-7 random likes from different users.
        var likes = new List<Like>();
        foreach (var post in posts)
        {
            var likers = users
                .OrderBy(_ => faker.Random.Int())
                .Take(faker.Random.Int(0, 7));

            foreach (var liker in likers)
            {
                likes.Add(new Like
                {
                    PostId = post.Id,
                    UserId = liker.Id,
                    CreatedAt = now.AddHours(-faker.Random.Int(1, 500)),
                });
            }
        }

        db.Set<Like>().AddRange(likes);
        await db.SaveChangesAsync();

        // --- Comments ---
        // 30 random posts get 1-3 comments each.
        var comments = new List<Comment>();
        foreach (var post in posts.OrderBy(_ => faker.Random.Int()).Take(30))
        {
            var count = faker.Random.Int(1, 3);
            for (var i = 0; i < count; i++)
            {
                var commenter = faker.PickRandom(users);
                comments.Add(new Comment
                {
                    Content = faker.Lorem.Sentence(faker.Random.Int(2, 8)),
                    PostId = post.Id,
                    AuthorId = commenter.Id,
                    // Comments are created after the post (5 mins to 24 hours later).
                    CreatedAt = post.CreatedAt.AddMinutes(faker.Random.Int(5, 1440)),
                });
            }
        }

        db.Comments.AddRange(comments);
        await db.SaveChangesAsync();
    }
}

using Blog.Domain.Entities;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure;

public static class FakeDataSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Users.AnyAsync()) return;

        var now = DateTime.UtcNow;
        Randomizer.Seed = new Random(42);

        // --- Users ---
        var userFaker = new Faker<User>()
            .RuleFor(u => u.UserName, f => f.Internet.UserName().ToLower().Replace(".", "_"))
            .RuleFor(u => u.Email, (f, u) => $"{u.UserName}@{f.Internet.DomainName()}")
            .RuleFor(u => u.PasswordHash, _ => BCrypt.Net.BCrypt.HashPassword("123456"))
            .RuleFor(u => u.Role, _ => "User")
            .RuleFor(u => u.Bio, f => f.PickRandom(
                f.Person.Company.Name + " | " + f.Hacker.IngVerb() + " stuff",
                f.Lorem.Sentence(4, 3),
                null
            ))
            .RuleFor(u => u.CreatedAt, f => now.AddDays(-f.Random.Int(30, 90)));

        var users = userFaker.Generate(10);
        db.Users.AddRange(users);
        await db.SaveChangesAsync();

        // --- Follows ---
        var faker = new Faker();
        var follows = new List<Follow>();
        foreach (var user in users)
        {
            var toFollow = users
                .Where(u => u.Id != user.Id)
                .OrderBy(_ => faker.Random.Int())
                .Take(faker.Random.Int(2, 5));

            foreach (var target in toFollow)
            {
                follows.Add(new Follow
                {
                    FollowerId = user.Id,
                    FollowingId = target.Id,
                    CreatedAt = now.AddDays(-faker.Random.Int(1, 30)),
                });
            }
        }

        db.Set<Follow>().AddRange(follows);
        await db.SaveChangesAsync();

        // --- Tags ---
        var tagNames = new[] { "dev", "dotnet", "react", "music", "gaming", "food", "travel", "tech", "memes", "fitness" };
        var tags = tagNames.Select(t => new Tag { Name = t }).ToList();
        db.Set<Tag>().AddRange(tags);
        await db.SaveChangesAsync();

        // --- Posts (Yaps) ---
        var yapFaker = new Faker();
        var posts = new List<Post>();

        for (var i = 0; i < 50; i++)
        {
            var sentence = yapFaker.Lorem.Sentence(yapFaker.Random.Int(3, 12));
            var numTags = yapFaker.Random.Int(0, 3);
            var postTags = yapFaker.PickRandom(tagNames, numTags);
            var hashtags = string.Join(" ", postTags.Select(t => $"#{t}"));
            var content = string.IsNullOrEmpty(hashtags) ? sentence : $"{sentence} {hashtags}";

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
        var postTagEntries = new List<PostTag>();
        foreach (var post in posts)
        {
            var hashtagMatches = System.Text.RegularExpressions.Regex.Matches(post.Content, @"#(\w+)");
            foreach (System.Text.RegularExpressions.Match match in hashtagMatches)
            {
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
                    CreatedAt = post.CreatedAt.AddMinutes(faker.Random.Int(5, 1440)),
                });
            }
        }

        db.Comments.AddRange(comments);
        await db.SaveChangesAsync();
    }
}

using Blog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Users.AnyAsync()) return;

        var now = DateTime.UtcNow;
        var random = new Random(42);

        // --- Users ---
        var users = new List<User>();
        var userNames = new[]
        {
            "alice", "bob", "carol", "dave", "eve",
            "frank", "grace", "heidi", "ivan", "judy"
        };
        var bios = new[]
        {
            "Full-stack dev. Coffee addict. Building cool stuff.",
            "Backend engineer by day, gamer by night.",
            "Design enthusiast and open source contributor.",
            "Learning something new every day. #dev",
            "Cybersecurity nerd. CTF player.",
            "DevOps engineer. Automating all the things.",
            "Frontend dev. Pixel perfectionist.",
            "Student. Aspiring software engineer.",
            "Systems programmer. Rust evangelist.",
            "QA engineer. Breaking things professionally.",
        };

        for (var i = 0; i < userNames.Length; i++)
        {
            users.Add(new User
            {
                UserName = userNames[i],
                Email = $"{userNames[i]}@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Role = "User",
                Bio = bios[i],
                CreatedAt = now.AddDays(-random.Next(30, 90)),
            });
        }

        db.Users.AddRange(users);
        await db.SaveChangesAsync();

        // --- Follows ---
        var follows = new List<Follow>();
        foreach (var user in users)
        {
            var toFollow = users
                .Where(u => u.Id != user.Id)
                .OrderBy(_ => random.Next())
                .Take(random.Next(2, 6));

            foreach (var target in toFollow)
            {
                follows.Add(new Follow
                {
                    FollowerId = user.Id,
                    FollowingId = target.Id,
                    CreatedAt = now.AddDays(-random.Next(1, 30)),
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
        var yapTemplates = new[]
        {
            "Just deployed my first API to production! #dev #dotnet",
            "React hooks finally clicked for me today #react #dev",
            "Nothing beats a good cup of coffee while coding",
            "Hot take: tabs are better than spaces",
            "Who else is addicted to mechanical keyboards? #tech",
            "Made pasta from scratch tonight #food",
            "Weekend hiking trip was amazing #travel",
            "Can't stop playing this new indie game #gaming",
            "Learning TypeScript was the best decision I made #dev #react",
            "Assembly is the only real programming language, change my mind #tech",
            "My cat just sat on my keyboard and pushed to production",
            "Day 30 of #fitness journey, feeling great!",
            "Just discovered daisyUI and it's a game changer #dev #react",
            "PostgreSQL > MySQL, fight me #dev #dotnet",
            "Is it too late to become a farmer? Asking for a friend",
            "New blog post about clean architecture patterns #dev #dotnet",
            "Anyone else refactor code at 3am? Just me? #dev",
            "The best error message is no error message #tech",
            "Pizza is a valid dinner for the 5th night in a row #food",
            "Finally understand dependency injection #dev #dotnet",
            "Spotify wrapped says I listened to 50k minutes of lo-fi #music",
            "Deployed on a Friday. Pray for me. #dev",
            "Just finished a 10k run #fitness",
            "Tailwind CSS changed my life #dev #react",
            "Sometimes the bug is a feature #dev #tech",
            "Road trip through the mountains this weekend #travel",
            "Retro games night with friends #gaming",
            "Microservices or monolith? The eternal debate #dev #tech",
            "Found the best ramen place in town #food",
            "REST vs GraphQL, round 347 #dev #tech",
            "Morning gym session hits different #fitness",
            "Writing unit tests saved me today #dev #dotnet",
            "The documentation was actually helpful for once #dev",
            "New mechanical keyboard just arrived #tech",
            "Sunset from the office window was beautiful today #travel",
            "Making beats on the weekend #music",
            "EF Core migrations are magic #dev #dotnet",
            "Just beat the final boss after 40 hours #gaming",
            "Homemade sushi attempt... it was edible #food",
            "Dark mode everything #tech #dev",
            "Code review is an act of love #dev",
            "Started learning Go this week #dev #tech",
            "Protein shake recipes anyone? #fitness #food",
            "Live concert tonight, so hyped! #music",
            "Backpacking through Europe next month #travel",
            "The rubber duck debugging method actually works #dev",
            "CI/CD pipeline finally green after 3 hours #dev #dotnet",
            "Sleep is overrated when you have side projects #dev #tech",
            "Best meme I've seen all week #memes",
            "Open source contributions feel amazing #dev #tech",
        };

        var posts = new List<Post>();
        foreach (var template in yapTemplates)
        {
            var author = users[random.Next(users.Count)];
            posts.Add(new Post
            {
                Content = template,
                AuthorId = author.Id,
                CreatedAt = now.AddHours(-random.Next(1, 720)),
            });
        }

        db.Posts.AddRange(posts);
        await db.SaveChangesAsync();

        // --- PostTags ---
        var postTags = new List<PostTag>();
        foreach (var post in posts)
        {
            var hashtagMatches = System.Text.RegularExpressions.Regex.Matches(post.Content, @"#(\w+)");
            foreach (System.Text.RegularExpressions.Match match in hashtagMatches)
            {
                var tag = tags.FirstOrDefault(t => t.Name.Equals(match.Groups[1].Value, StringComparison.OrdinalIgnoreCase));
                if (tag != null)
                {
                    postTags.Add(new PostTag { PostId = post.Id, TagId = tag.Id });
                }
            }
        }

        db.Set<PostTag>().AddRange(postTags);
        await db.SaveChangesAsync();

        // --- Likes ---
        var likes = new List<Like>();
        foreach (var post in posts)
        {
            var likers = users
                .OrderBy(_ => random.Next())
                .Take(random.Next(0, 7));

            foreach (var liker in likers)
            {
                likes.Add(new Like
                {
                    PostId = post.Id,
                    UserId = liker.Id,
                    CreatedAt = now.AddHours(-random.Next(1, 500)),
                });
            }
        }

        db.Set<Like>().AddRange(likes);
        await db.SaveChangesAsync();

        // --- Comments ---
        var commentTemplates = new[]
        {
            "Totally agree!", "This is so true!", "Great point!",
            "Haha love this", "Same here!", "Couldn't agree more",
            "Nice one!", "Facts!", "This made my day",
            "Relatable content", "Keep it up!", "Interesting take",
        };

        var comments = new List<Comment>();
        foreach (var post in posts.OrderBy(_ => random.Next()).Take(30))
        {
            var count = random.Next(1, 4);
            for (var i = 0; i < count; i++)
            {
                var commenter = users[random.Next(users.Count)];
                comments.Add(new Comment
                {
                    Content = commentTemplates[random.Next(commentTemplates.Length)],
                    PostId = post.Id,
                    AuthorId = commenter.Id,
                    CreatedAt = post.CreatedAt.AddMinutes(random.Next(5, 1440)),
                });
            }
        }

        db.Comments.AddRange(comments);
        await db.SaveChangesAsync();
    }
}

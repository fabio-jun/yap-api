using System.Reflection;
using System.Text;
using Blog.API.Middlewares;
using Blog.Infrastructure;
using Blog.Infrastructure.Repositories;
using Blog.Domain.Interfaces;
using Blog.Application.Interfaces;
using Blog.Application.Services;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

// --- APPLICATION BUILDER PHASE ---
// WebApplication.CreateBuilder — creates the host with default configuration:
// loads appsettings.json, environment variables, command-line args, and sets up logging.
// Uses the "minimal hosting model" (no Startup class) introduced in .NET 6.
var builder = WebApplication.CreateBuilder(args);

// Configures Kestrel (the built-in web server) to accept request bodies up to 55MB.
// Needed for file uploads (images/videos via Cloudinary).
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 55 * 1024 * 1024; // 55MB
});

// --- CORS Configuration ---
// GetSection().Get<string[]>() — binds a JSON array from appsettings to a C# string[].
// ?? — null-coalescing operator: if config is null, use these default origins.
// Collection expression [...] — C# 12 syntax for array initialization.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173", "http://localhost:3000", "https://yap-client.vercel.app"];

// AddCors — registers CORS services. The policy name "AllowFrontend" is referenced later in UseCors.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// --- SERVICE REGISTRATION (Dependency Injection Container) ---

// AddControllers — registers all [ApiController] classes and their dependencies.
// This enables attribute routing ([Route], [HttpGet], etc.) and model binding.
builder.Services.AddControllers();

// AddDbContext — registers AppDbContext as a Scoped service (one instance per HTTP request).
// UseNpgsql — configures EF Core to use PostgreSQL via the Npgsql provider.
// GetConnectionString("DefaultConnection") — reads from appsettings.json > ConnectionStrings > DefaultConnection.
builder.Services.AddDbContext<AppDbContext>(options =>
      options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Swagger/OpenAPI — auto-generates API documentation at /swagger.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Yap API",
        Version = "v1",
        Description = "REST API for Yap, a social networking platform built with ASP.NET Core and Entity Framework Core.",
    });

    options.EnableAnnotations();

    var apiXml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(apiXml))
    {
        options.IncludeXmlComments(apiXml);
    }

    var applicationXml = Path.Combine(AppContext.BaseDirectory, "Blog.Application.xml");
    if (File.Exists(applicationXml))
    {
        options.IncludeXmlComments(applicationXml);
    }

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: Bearer eyJhbGciOi...",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(openApiDocument => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", openApiDocument, null),
            new List<string>()
        }
    });
});

// --- JWT Authentication ---
// AddAuthentication — registers the authentication services with JWT Bearer as the default scheme.
// JwtBearerDefaults.AuthenticationScheme = "Bearer" — the scheme name used in [Authorize] attributes.
// AddJwtBearer — configures how incoming JWT tokens are validated.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(options =>
      {
          // TokenValidationParameters — defines what to check in each incoming token.
          options.TokenValidationParameters = new TokenValidationParameters
          {
              ValidateIssuer = true,           // Check token was issued by our server
              ValidateAudience = true,         // Check token is intended for our API
              ValidateLifetime = true,         // Check token hasn't expired
              ValidateIssuerSigningKey = true,  // Verify the signature with our secret key
              ValidIssuer = builder.Configuration["Jwt:Issuer"],
              ValidAudience = builder.Configuration["Jwt:Audience"],
              // SymmetricSecurityKey — same key is used for signing and verifying (HMAC-SHA256).
              // The '!' (null-forgiving operator) asserts the config value exists at runtime.
              IssuerSigningKey = new SymmetricSecurityKey(
                  Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
          };
      });

// --- Scoped Service Registration ---
// AddScoped — creates a new instance per HTTP request. All services in the same request share instances.
// Pattern: AddScoped<Interface, Implementation>() — binds the abstraction to the concrete class.
// When a controller requests IPostService, DI provides a PostService instance.

// Repositories (data access layer)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<ILikeRepository, LikeRepository>();
builder.Services.AddScoped<IFollowRepository, FollowRepository>();
builder.Services.AddScoped<IBookmarkRepository, BookmarkRepository>();
builder.Services.AddScoped<IDirectMessageRepository, DirectMessageRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IRepostRepository, RepostRepository>();
builder.Services.AddScoped<IBlockRepository, BlockRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();

// Application services (business logic layer)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<ILikeService, LikeService>();
builder.Services.AddScoped<IFollowService, FollowService>();
builder.Services.AddScoped<IBookmarkService, BookmarkService>();
builder.Services.AddScoped<IDirectMessageService, DirectMessageService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IRepostService, RepostService>();
builder.Services.AddScoped<IBlockService, BlockService>();
builder.Services.AddScoped<IReportService, ReportService>();

// --- Cloudinary Configuration ---
// AddSingleton — creates one instance shared across all requests (Cloudinary client is thread-safe).
// Reads the Cloudinary URL from configuration; falls back to a demo account if not set.
var cloudinaryUrl = builder.Configuration["Cloudinary:Url"];
if (!string.IsNullOrEmpty(cloudinaryUrl))
{
    builder.Services.AddSingleton(new Cloudinary(cloudinaryUrl));
}
else
{
    builder.Services.AddSingleton(new Cloudinary(new Account("demo", "demo", "demo")));
}

// --- APPLICATION PIPELINE PHASE ---
// Build() — finalizes the service container and creates the WebApplication.
// After this point, no more services can be registered.
var app = builder.Build();

// --- Database Migration & Seeding ---
// CreateScope() — creates a temporary DI scope to resolve scoped services outside of an HTTP request.
// using — ensures the scope is disposed after the block (C# IDisposable pattern).
// Migrate() — applies any pending EF Core migrations to the database (creates/alters tables).
// SeedAsync — populates the database with fake data if empty (see FakeDataSeeder).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await FakeDataSeeder.SeedAsync(db);
}

// --- Middleware Pipeline ---
// The order of middleware matters! Each request flows through these in order.

// CORS must be before auth and controllers.
app.UseCors("AllowFrontend");

// Global exception handler — catches all unhandled exceptions and returns JSON error responses.
app.UseMiddleware<ExceptionMiddleware>();

// Swagger UI — only enabled in Development environment (not exposed in production).
if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS redirection — only in development (production handles this at the reverse proxy/load balancer level).
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Authentication must come before Authorization.
// UseAuthentication — reads and validates the JWT token from the Authorization header.
app.UseAuthentication();

// UseAuthorization — enforces [Authorize] attributes on controllers/actions.
app.UseAuthorization();

// --- Static Files ---
// Ensures the wwwroot/uploads directory exists for local file uploads.
// UseStaticFiles — serves files from wwwroot/ (images, uploads, etc.) as HTTP responses.
var webRootPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
Directory.CreateDirectory(Path.Combine(webRootPath, "uploads"));
app.Environment.WebRootPath = webRootPath;
app.UseStaticFiles();

// MapControllers — maps attribute-routed controllers to the endpoint routing system.
// This connects [Route("api/[controller]")] and [HttpGet] attributes to the pipeline.
app.MapControllers();

// Run — starts the Kestrel web server and blocks until shutdown (Ctrl+C or SIGTERM).
app.Run();

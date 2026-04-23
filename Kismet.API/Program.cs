using Kismet.Core.Helpers;
using Kismet.Infrastructure;
using Kismet.Repository;

var builder = WebApplication.CreateBuilder(args);

// Host lifetime is tied to the console (Ctrl+C / console close)
builder.Host.UseConsoleLifetime();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // DateOnly is natively supported in .NET 9.0 and serializes as ISO 8601 date format (YYYY-MM-DD)
        options.JsonSerializerOptions.WriteIndented = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

const string LocalCorsPolicy = "LocalCorsPolicy";
var allowedOrigins = new[]
{
    "http://localhost:3055",
    "https://localhost:3055",
    "http://localhost:5173",  // Vite dev server
    "https://localhost:5173"
};

builder.Services
    .AddInfrastructure(builder.Configuration)  // Registers SqlConnectionFactory
    .AddRepositories();                        // Registers repositories

// Register JwtHelper to create JWT tokens
builder.Services.AddScoped<JwtHelper>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(LocalCorsPolicy, policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Required for cookies
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

//aaa app.UseHttpsRedirection();

app.UseCors(LocalCorsPolicy);

// Middleware to read access token from cookie and add to Authorization header
app.UseMiddleware<Kismet.API.Middleware.CookieTokenMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger/index.html", true, true)).AllowAnonymous();

// Force HTTP for development
app.Run("http://localhost:3055/");
using Kismet.Infrastructure;
using Kismet.Repository;

var builder = WebApplication.CreateBuilder(args);

// Host lifetime is tied to the console (Ctrl+C / console close)
builder.Host.UseConsoleLifetime();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

const string LocalCorsPolicy = "LocalCorsPolicy";
var allowedOrigins = new[]
{
    "http://localhost:3055",
    "https://localhost:3055"
};

builder.Services
    .AddInfrastructure(builder.Configuration)  // Registers SqlConnectionFactory
    .AddRepositories();                        // Registers repositories

builder.Services.AddCors(options =>
{
    options.AddPolicy(LocalCorsPolicy, policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

//aaa app.UseHttpsRedirection();

app.UseCors(LocalCorsPolicy);

//app.UseAuthorization();

app.MapControllers();

//aaa app.MapGet("/", () => Results.Redirect("/swagger/index.html", true, true)).AllowAnonymous();

// Force HTTP for development
app.Run("http://localhost:3055/");
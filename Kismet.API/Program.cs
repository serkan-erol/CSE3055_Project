using Kismet.Bussiness;
using Kismet.DataAccess;
using Kismet.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

const string LocalCorsPolicy = "LocalCorsPolicy";
var allowedOrigins = new[]
{
    "http://localhost:5098",
    "https://localhost:5098"
};

builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddDataAccess(builder.Configuration)
    .AddBusinessServices();

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

app.UseHttpsRedirection();

app.UseCors(LocalCorsPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();

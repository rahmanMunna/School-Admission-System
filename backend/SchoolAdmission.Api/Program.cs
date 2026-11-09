using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Api.Middleware;
using SchoolAdmission.Business.Errors;
using SchoolAdmission.Business.Options;
using SchoolAdmission.Business.Services;
using SchoolAdmission.Data;

var builder = WebApplication.CreateBuilder(args);

// ---------- Configuration (environment variables override appsettings) ----------
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException(
        "Missing database connection string. Set the ConnectionStrings__Default environment variable.");

var holdSeconds = int.TryParse(builder.Configuration["HOLD_DURATION_SECONDS"], out var seconds) && seconds > 0
    ? seconds
    : HoldOptions.DefaultSeconds;

// ---------- Tier 3: Data access ----------
builder.Services.AddDataAccess(connectionString);

// ---------- Tier 2: Business ----------
builder.Services.Configure<HoldOptions>(o => o.HoldDurationSeconds = holdSeconds);
builder.Services.AddSingleton(TimeProvider.System);   // injectable clock (tests can fake it)
// Service implementations (ClassService, HoldService) get registered in the services step.
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<IHoldService, HoldService>();

// ---------- Tier 1: Presentation ----------
builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = _ => new BadRequestObjectResult(
            new ErrorResponse(ErrorCodes.ValidationError,
                "The request is not valid. Please check the fields and try again."));
    });

builder.Services.AddOpenApi();

// The React app runs on another port, so the browser needs CORS.
// AllowAnyOrigin is safe here: no cookies/credentials are used.
// It must allow the custom X-Student-Id header and DELETE.
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .WithMethods("GET", "POST", "DELETE")));

var app = builder.Build();

app.Logger.LogInformation("Hold duration: {Seconds} seconds", holdSeconds);

// Create/upgrade the tables before accepting requests.
await app.Services.MigrateDatabaseAsync(app.Logger);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();                                  // FIRST, so error responses also get CORS headers
app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

await app.RunAsync();

public partial class Program { }
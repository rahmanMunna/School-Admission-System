using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Api.Contracts;
using SchoolAdmission.Api.Errors;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Bad JSON or wrong types (e.g. "totalSeats": "abc") → OUR error format, not ASP.NET's default.
        options.InvalidModelStateResponseFactory = _ => new BadRequestObjectResult(
            new ErrorResponse(ErrorCodes.ValidationError,
                "The request is not valid. Please check the fields and try again."));
    });

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ErrorHandlingMiddleware>();   // catches all errors → { error, message }

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

public partial class Program { }
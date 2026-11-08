using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Api.Middleware;
using SchoolAdmission.Business.Errors;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Bad JSON / wrong types (e.g. "totalSeats": "abc") → OUR error format.
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

app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

public partial class Program { }
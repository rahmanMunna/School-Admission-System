using System.Text.Json.Serialization;

namespace SchoolAdmission.Api.Middleware;

// The HTTP error body for every endpoint: { "error": "CODE", "message": "Human text" }
public record ErrorResponse(
    string Error,
    string Message,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] object? Details = null);
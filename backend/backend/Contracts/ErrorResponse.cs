using System.Text.Json.Serialization;

namespace SchoolAdmission.Api.Contracts;

// One error format for every endpoint: { "error": "CODE", "message": "Human text" }
public record ErrorResponse(
    string Error,
    string Message,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] object? Details = null);
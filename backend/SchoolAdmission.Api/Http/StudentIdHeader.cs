using SchoolAdmission.Business.Validation;

namespace SchoolAdmission.Api.Http;

// Presentation concern: READ the header. The rule itself lives in the Business tier.
public static class StudentIdHeader
{
    public const string Name = "X-Student-Id";

    // Required: missing or invalid → 400 (thrown by StudentIdRules)
    public static string Require(HttpRequest request) =>
        StudentIdRules.Validate(request.Headers[Name].ToString());

    // Optional (GET /holds/{id}): validated only if sent
    public static string? Optional(HttpRequest request) =>
        request.Headers.ContainsKey(Name) ? Require(request) : null;
}
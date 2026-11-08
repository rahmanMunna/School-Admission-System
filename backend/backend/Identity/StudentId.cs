using System.Text.RegularExpressions;
using SchoolAdmission.Api.Errors;

namespace SchoolAdmission.Api.Identity;

// Reads and validates the X-Student-Id header (17-digit birth registration number).
public static partial class StudentId
{
    public const string HeaderName = "X-Student-Id";

    // [0-9] on purpose: in .NET, \d also matches Bangla and other Unicode digits.
    [GeneratedRegex("^[0-9]{17}$")]
    private static partial Regex Pattern();

    // For endpoints where the header is required.
    public static string Require(HttpRequest request)
    {
        var value = request.Headers[HeaderName].ToString().Trim();

        if (value.Length == 0)
            throw ApiException.BadRequest(ErrorCodes.MissingStudentId,
                "Please enter the student's birth registration number.");

        if (!Pattern().IsMatch(value))
            throw ApiException.BadRequest(ErrorCodes.InvalidStudentId,
                "Birth registration number must be exactly 17 digits.");

        return value;
    }

    // For GET /holds/{id}: the header is optional, but must be valid if it is sent.
    public static string? Optional(HttpRequest request) =>
        request.Headers.ContainsKey(HeaderName) ? Require(request) : null;

    // For logs: never write the full number.
    public static string Mask(string id) =>
        id.Length == 17 ? $"{id[..4]}*********{id[^4..]}" : "***";
}
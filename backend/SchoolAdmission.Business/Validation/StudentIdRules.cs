using System.Text.RegularExpressions;
using SchoolAdmission.Business.Errors;

namespace SchoolAdmission.Business.Validation;

// The business rule for a student ID: exactly 17 ASCII digits.
// Lives in the Business tier; the Api tier only reads the header and calls this.
public static partial class StudentIdRules
{
    // [0-9] on purpose: in .NET, \d also matches Bangla and other Unicode digits.
    [GeneratedRegex("^[0-9]{17}$")]
    private static partial Regex Pattern();

    public static string Validate(string? raw)
    {
        var value = raw?.Trim() ?? string.Empty;

        if (value.Length == 0)
            throw AppException.Validation(ErrorCodes.MissingStudentId,
                "Please enter the student's birth registration number.");

        if (!Pattern().IsMatch(value))
            throw AppException.Validation(ErrorCodes.InvalidStudentId,
                "Birth registration number must be exactly 17 digits.");

        return value;
    }

    // For logs: never write the full number.
    public static string Mask(string id) =>
        id.Length == 17 ? $"{id[..4]}*********{id[^4..]}" : "***";
}
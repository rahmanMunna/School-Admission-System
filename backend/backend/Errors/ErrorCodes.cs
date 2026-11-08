namespace SchoolAdmission.Api.Errors;

// Machine-readable codes. The UI uses these to decide what to show.
public static class ErrorCodes
{
    public const string ValidationError = "VALIDATION_ERROR";
    public const string InvalidName = "INVALID_NAME";
    public const string InvalidTotalSeats = "INVALID_TOTAL_SEATS";
    public const string MissingStudentId = "MISSING_STUDENT_ID";
    public const string InvalidStudentId = "INVALID_STUDENT_ID";
    public const string ClassNotFound = "CLASS_NOT_FOUND";
    public const string HoldNotFound = "HOLD_NOT_FOUND";
    public const string NotYourHold = "NOT_YOUR_HOLD";
    public const string NoSeatsAvailable = "NO_SEATS_AVAILABLE";
    public const string AlreadyApplied = "ALREADY_APPLIED";
    public const string HoldExpired = "HOLD_EXPIRED";
    public const string HoldCancelled = "HOLD_CANCELLED";
    public const string CannotCancelConfirmed = "CANNOT_CANCEL_CONFIRMED";
    public const string InternalError = "INTERNAL_ERROR";
}
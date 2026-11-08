namespace SchoolAdmission.Business.Errors;

// Thrown by services when a business rule fails.
// No HTTP status codes here; the Api tier translates ErrorType → status code.
public sealed class AppException : Exception
{
    public ErrorType Type { get; }
    public string Code { get; }
    public object? Details { get; }

    private AppException(ErrorType type, string code, string message, object? details = null)
        : base(message)
    {
        Type = type;
        Code = code;
        Details = details;
    }

    public static AppException Validation(string code, string message) =>
        new(ErrorType.Validation, code, message);

    public static AppException Forbidden(string code, string message) =>
        new(ErrorType.Forbidden, code, message);

    public static AppException NotFound(string code, string message) =>
        new(ErrorType.NotFound, code, message);

    public static AppException Conflict(string code, string message, object? details = null) =>
        new(ErrorType.Conflict, code, message, details);
}
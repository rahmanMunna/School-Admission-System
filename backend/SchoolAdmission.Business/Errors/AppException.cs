namespace SchoolAdmission.Business.Errors;

// Thrown by services when a business rule fails.
// No HTTP status codes here; the Api tier translates ErrorType into a status code.
public sealed class AppException : Exception
{
    public ErrorType Type { get; }
    public string Code { get; }
    public object? Details { get; }

    // Private: other code can't call "new AppException(...)" directly.
    // It must use one of the four methods below, so every error has a clear type.
    private AppException(ErrorType type, string code, string message, object? details)
        : base(message)
    {
        Type = type;
        Code = code;
        Details = details;
    }

    // 400: the input is wrong (e.g. student ID not 17 digits)
    public static AppException Validation(string code, string message)
    {
        return new AppException(ErrorType.Validation, code, message, null);
    }

    // 403: it belongs to someone else
    public static AppException Forbidden(string code, string message)
    {
        return new AppException(ErrorType.Forbidden, code, message, null);
    }

    // 404: it doesn't exist
    public static AppException NotFound(string code, string message)
    {
        return new AppException(ErrorType.NotFound, code, message, null);
    }

    // 409: a business rule blocks it (class full, already applied, expired...)
    public static AppException Conflict(string code, string message, object? details = null)
    {
        return new AppException(ErrorType.Conflict, code, message, details);
    }
}
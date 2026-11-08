namespace SchoolAdmission.Api.Errors;

// Services throw this. The middleware turns it into the right HTTP status + JSON body.
public sealed class ApiException : Exception
{
    public int StatusCode { get; }
    public string Code { get; }
    public object? Details { get; }

    public ApiException(int statusCode, string code, string message, object? details = null)
        : base(message)
    {
        StatusCode = statusCode;
        Code = code;
        Details = details;
    }

    public static ApiException BadRequest(string code, string message) =>
        new(StatusCodes.Status400BadRequest, code, message);

    public static ApiException Forbidden(string code, string message) =>
        new(StatusCodes.Status403Forbidden, code, message);

    public static ApiException NotFound(string code, string message) =>
        new(StatusCodes.Status404NotFound, code, message);

    public static ApiException Conflict(string code, string message, object? details = null) =>
        new(StatusCodes.Status409Conflict, code, message, details);
}
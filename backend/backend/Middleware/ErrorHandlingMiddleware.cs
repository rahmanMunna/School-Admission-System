using SchoolAdmission.Business.Errors;

namespace SchoolAdmission.Api.Middleware;

// Translates business errors (AppException) into HTTP status codes + JSON.
// Unexpected errors become 500 with a safe message (no stack traces leak).
public sealed class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AppException ex)
        {
            await WriteAsync(context, ToStatusCode(ex.Type), new ErrorResponse(ex.Code, ex.Message, ex.Details));
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // The client disconnected. Nobody is listening, so send nothing.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error on {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteAsync(context, StatusCodes.Status500InternalServerError,
                new ErrorResponse(ErrorCodes.InternalError, "Something went wrong on our side. Please try again."));
        }
    }

    private static int ToStatusCode(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };

    private static Task WriteAsync(HttpContext context, int statusCode, ErrorResponse body)
    {
        if (context.Response.HasStarted) return Task.CompletedTask;
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(body);
    }
}
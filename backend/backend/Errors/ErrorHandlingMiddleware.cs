using SchoolAdmission.Api.Contracts;

namespace SchoolAdmission.Api.Errors;

// Catches every exception and returns { error, message } JSON.
// Result: no stack traces leak, and the UI always gets the same error shape.
public sealed class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ApiException ex)
        {
            await WriteAsync(context, ex.StatusCode, new ErrorResponse(ex.Code, ex.Message, ex.Details));
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

    private static Task WriteAsync(HttpContext context, int statusCode, ErrorResponse body)
    {
        if (context.Response.HasStarted) return Task.CompletedTask;
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(body);
    }
}
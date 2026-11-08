namespace SchoolAdmission.Business.Errors;

// Business-level error categories. The Api tier maps them to HTTP status codes,
// so the Business tier never needs to know about HTTP.
public enum ErrorType
{
    Validation,   // → 400
    Forbidden,    // → 403
    NotFound,     // → 404
    Conflict      // → 409
}
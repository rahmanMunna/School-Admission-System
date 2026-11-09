namespace SchoolAdmission.Business.Errors;
public enum ErrorType
{
    Validation,   // → 400
    Forbidden,    // → 403
    NotFound,     // → 404
    Conflict      // → 409
}
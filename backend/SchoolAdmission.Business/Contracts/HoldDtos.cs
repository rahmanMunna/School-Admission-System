namespace SchoolAdmission.Business.Contracts;

// 201 response of POST /classes/{classId}/holds. ServerTime is an allowed EXTRA field.
public record HoldCreatedResponse(
    Guid HoldId,
    Guid ClassId,
    string StudentId,
    string Status,
    DateTime ExpiresAt,
    DateTime ServerTime);

// GET /holds/{holdId}. ClassName and ServerTime are extra. No StudentId (no header on this endpoint).
public record HoldResponse(
    Guid HoldId,
    Guid ClassId,
    string ClassName,
    string Status,
    DateTime ExpiresAt,
    DateTime ServerTime);

// 200 response of POST /holds/{holdId}/confirm
public record ConfirmResponse(Guid HoldId, string Status);

// GET /me/holds (array of these)
public record MyHoldResponse(
    Guid HoldId,
    Guid ClassId,
    string ClassName,
    string Status,
    DateTime ExpiresAt);
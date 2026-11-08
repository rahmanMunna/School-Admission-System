namespace SchoolAdmission.Api.Contracts;

// 201 response of POST /classes/{classId}/holds
// ServerTime is an EXTRA field (allowed by the brief) so the UI countdown is accurate.
public record HoldCreatedResponse(
    Guid HoldId,
    Guid ClassId,
    string StudentId,
    string Status,
    DateTime ExpiresAt,
    DateTime ServerTime);

// GET /holds/{holdId}. ClassName and ServerTime are extra fields.
// No StudentId here: this endpoint has no header, so we don't leak it.
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
namespace SchoolAdmission.Business.Contracts;

// POST /classes body. Nullable so we can detect "missing" and return our own 400.
public record CreateClassRequest(string? Name, int? TotalSeats);

// 201 response of POST /classes
public record ClassCreatedResponse(Guid Id, string Name, int TotalSeats);

// GET /classes and GET /classes/{classId}
public record ClassResponse(
    Guid Id,
    string Name,
    int TotalSeats,
    int HeldSeats,
    int ConfirmedSeats,
    int AvailableSeats);
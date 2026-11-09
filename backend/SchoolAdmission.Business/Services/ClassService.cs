using SchoolAdmission.Business.Contracts;
using SchoolAdmission.Business.Errors;
using SchoolAdmission.Data.Entities;
using SchoolAdmission.Data.Repositories;

namespace SchoolAdmission.Business.Services;

public class ClassService(IClassRepository classes, IUnitOfWork unitOfWork, TimeProvider clock) : IClassService
{
    public async Task<ClassCreatedResponse> CreateAsync(CreateClassRequest request, CancellationToken ct)
    {
        var name = request.Name?.Trim();

        if (string.IsNullOrEmpty(name) || name.Length > 100)
            throw AppException.Validation(ErrorCodes.InvalidName,
                "Class name is required (maximum 100 characters).");

        if (request.TotalSeats is null or < 1 or > 10000)
            throw AppException.Validation(ErrorCodes.InvalidTotalSeats,
                "Total seats must be a whole number between 1 and 10000.");

        var schoolClass = new SchoolClass
        {
            Id = Guid.NewGuid(),
            Name = name,
            TotalSeats = request.TotalSeats.Value,
            CreatedAt = clock.UtcNowSeconds()
        };

        classes.Add(schoolClass);
        await unitOfWork.SaveChangesAsync(ct);

        return new ClassCreatedResponse(schoolClass.Id, schoolClass.Name, schoolClass.TotalSeats);
    }

    public async Task<IReadOnlyList<ClassResponse>> ListAsync(CancellationToken ct)
    {
        var rows = await classes.ListWithSeatCountsAsync(clock.UtcNowSeconds(), ct);
        return rows.Select(ToResponse).ToList();
    }

    public async Task<ClassResponse> GetAsync(string classId, CancellationToken ct)
    {
        if (!Guid.TryParse(classId, out var id))
            throw AppException.NotFound(ErrorCodes.ClassNotFound, "This class does not exist.");

        var row = await classes.GetWithSeatCountsAsync(id, clock.UtcNowSeconds(), ct)
                  ?? throw AppException.NotFound(ErrorCodes.ClassNotFound, "This class does not exist.");

        return ToResponse(row);
    }

    private static ClassResponse ToResponse(ClassSeatCounts c)
    {
        var available = Math.Max(0, c.TotalSeats - c.HeldSeats - c.ConfirmedSeats);
        return new ClassResponse(c.Id, c.Name, c.TotalSeats, c.HeldSeats, c.ConfirmedSeats, available);
    }
}
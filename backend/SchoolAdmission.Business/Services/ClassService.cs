using SchoolAdmission.Business.Contracts;
using SchoolAdmission.Business.Errors;
using SchoolAdmission.Data.Entities;
using SchoolAdmission.Data.Repositories;

namespace SchoolAdmission.Business.Services;

public class ClassService : IClassService
{
    private readonly IClassRepository _classes;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _clock;

    public ClassService(IClassRepository classes,IUnitOfWork unitOfWork,TimeProvider clock)
    {
        _classes = classes;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<ClassCreatedResponse> CreateAsync(CreateClassRequest request,CancellationToken ct)
    {
        var name = request.Name?.Trim();

        if (string.IsNullOrEmpty(name) || name.Length > 100)
        {
            throw AppException.Validation(ErrorCodes.InvalidName,"Class name is required (maximum 100 characters).");
        }

        if (request.TotalSeats is null or < 1 or > 10000)
        {
            throw AppException.Validation(ErrorCodes.InvalidTotalSeats,"Total seats must be a whole number between 1 and 10000.");
        }

        var schoolClass = new SchoolClass
        {
            Id = Guid.NewGuid(),
            Name = name,
            TotalSeats = request.TotalSeats.Value,
            CreatedAt = _clock.UtcNowSeconds()
        };

        _classes.Add(schoolClass);

        await _unitOfWork.SaveChangesAsync(ct);

        var response = new ClassCreatedResponse
            (
                schoolClass.Id,
                schoolClass.Name,
                schoolClass.TotalSeats
            );

        return response;
    }

    public async Task<IReadOnlyList<ClassResponse>> ListAsync(
        CancellationToken ct)
    {
        // Get all classes with their seat counts
        var rows = await _classes.ListWithSeatCountsAsync(
            _clock.UtcNowSeconds(),
            ct);

        // Convert repository results into API responses
        var result = rows.Select(ToResponse).ToList();

        return result;
    }

    public async Task<ClassResponse> GetAsync(
        string classId,
        CancellationToken ct)
    {
        // Convert string ID into Guid
        if (!Guid.TryParse(classId, out var id))
        {
            throw AppException.NotFound(
                ErrorCodes.ClassNotFound,
                "This class does not exist.");
        }

        // Find class with its seat counts
        var row = await _classes.GetWithSeatCountsAsync(
            id,
            _clock.UtcNowSeconds(),
            ct);

        if (row == null)
        {
            throw AppException.NotFound(
                ErrorCodes.ClassNotFound,
                "This class does not exist.");
        }

        // Convert repository result into API response
        var response = ToResponse(row);

        return response;
    }

    // Convert ClassSeatCounts into ClassResponse
    private static ClassResponse ToResponse(ClassSeatCounts c)
    {
        var available = Math.Max(
            0,
            c.TotalSeats - c.HeldSeats - c.ConfirmedSeats);

        var response = new ClassResponse(
            c.Id,
            c.Name,
            c.TotalSeats,
            c.HeldSeats,
            c.ConfirmedSeats,
            available);

        return response;
    }
}
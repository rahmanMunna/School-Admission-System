using SchoolAdmission.Business.Contracts;

namespace SchoolAdmission.Business.Services;

public interface IClassService
{
    Task<ClassCreatedResponse> CreateAsync(CreateClassRequest request, CancellationToken ct);
    Task<IReadOnlyList<ClassResponse>> ListAsync(CancellationToken ct);
    Task<ClassResponse> GetAsync(string classId, CancellationToken ct);
}
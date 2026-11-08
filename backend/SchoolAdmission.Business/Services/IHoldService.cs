using SchoolAdmission.Business.Contracts;

namespace SchoolAdmission.Business.Services;

public interface IHoldService
{
    Task<HoldCreatedResponse> CreateAsync(string classId, string studentId, CancellationToken ct);
    Task<HoldResponse> GetAsync(string holdId, string? studentId, CancellationToken ct);
    Task<ConfirmResponse> ConfirmAsync(string holdId, string studentId, CancellationToken ct);
    Task CancelAsync(string holdId, string studentId, CancellationToken ct);
    Task<IReadOnlyList<MyHoldResponse>> ListMineAsync(string studentId, CancellationToken ct);
}
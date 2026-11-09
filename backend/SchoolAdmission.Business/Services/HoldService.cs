using Microsoft.Extensions.Options;
using SchoolAdmission.Business.Contracts;
using SchoolAdmission.Business.Errors;
using SchoolAdmission.Business.Options;
using SchoolAdmission.Data.Entities;
using SchoolAdmission.Data.Repositories;

namespace SchoolAdmission.Business.Services;

public class HoldService(
    IClassRepository classes,
    IHoldRepository holds,
    IUnitOfWork unitOfWork,
    TimeProvider clock,
    IOptions<HoldOptions> options) : IHoldService
{
    // ---------- Apply for a seat ----------
    public async Task<HoldCreatedResponse> CreateAsync(string classIdText, string studentId, CancellationToken ct)
    {
        if (!Guid.TryParse(classIdText, out var classId))
            throw AppException.NotFound(ErrorCodes.ClassNotFound, "This class does not exist.");

        var now = clock.UtcNowSeconds();

        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);

        // 1. Lock the class row. Other requests for THIS class wait here until we commit.
        var schoolClass = await classes.GetForUpdateAsync(classId, ct)
            ?? throw AppException.NotFound(ErrorCodes.ClassNotFound, "This class does not exist.");

        // 2. Mark this class's time-expired holds as EXPIRED.
        await holds.ExpireStaleForClassAsync(classId, now, ct);

        // 3. One active application per student per class.
        var existingHoldId = await holds.FindActiveHoldIdAsync(classId, studentId, now, ct);
        if (existingHoldId is not null)
            throw AppException.Conflict(ErrorCodes.AlreadyApplied,
                "This student already has an active application for this class.",
                new { holdId = existingHoldId });

        // 4. Is there a free seat?
        var usedSeats = await holds.CountActiveAsync(classId, now, ct);
        if (usedSeats >= schoolClass.TotalSeats)
            throw AppException.Conflict(ErrorCodes.NoSeatsAvailable,
                "Sorry, all seats in this class are taken right now.");

        // 5. Create the hold and commit (commit releases the lock).
        var hold = new Hold
        {
            Id = Guid.NewGuid(),
            ClassId = classId,
            StudentId = studentId,
            Status = HoldStatus.HELD,
            CreatedAt = now,
            ExpiresAt = now.AddSeconds(options.Value.HoldDurationSeconds)
        };

        holds.Add(hold);
        await unitOfWork.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return new HoldCreatedResponse(hold.Id, classId, studentId, "HELD", hold.ExpiresAt, now);
    }

    // ---------- View one application ----------
    public async Task<HoldResponse> GetAsync(string holdIdText, string? studentId, CancellationToken ct)
    {
        var hold = await FindHoldAsync(holdIdText, ct);

        if (studentId is not null && hold.StudentId != studentId)
            throw AppException.Forbidden(ErrorCodes.NotYourHold,
                "This application belongs to a different student.");

        var now = clock.UtcNowSeconds();
        return new HoldResponse(hold.Id, hold.ClassId, hold.Class.Name,
            StatusAt(hold, now).ToString(), hold.ExpiresAt, now);
    }

    // ---------- Pay (confirm) ----------
    public async Task<ConfirmResponse> ConfirmAsync(string holdIdText, string studentId, CancellationToken ct)
    {
        var hold = await FindHoldAsync(holdIdText, ct);
        var now = clock.UtcNowSeconds();

        // One UPDATE … WHERE status = 'HELD' AND not expired. Only one call can ever win.
        if (await holds.TryConfirmAsync(hold.Id, studentId, now, ct))
            return new ConfirmResponse(hold.Id, "CONFIRMED");

        // Nothing changed. Find out why.
        if (hold.StudentId != studentId)
            throw AppException.Forbidden(ErrorCodes.NotYourHold,
                "This application belongs to a different student.");

        var current = await FindHoldAsync(holdIdText, ct);      // fresh read
        var status = StatusAt(current, now);

        if (status == HoldStatus.CONFIRMED)                      // already paid → same answer (idempotent)
            return new ConfirmResponse(hold.Id, "CONFIRMED");

        if (status == HoldStatus.CANCELLED)
            throw AppException.Conflict(ErrorCodes.HoldCancelled, "This application was cancelled.");

        throw AppException.Conflict(ErrorCodes.HoldExpired,
            "Time is up. This application expired before payment. Please apply again.");
    }

    // ---------- Cancel ----------
    public async Task CancelAsync(string holdIdText, string studentId, CancellationToken ct)
    {
        var hold = await FindHoldAsync(holdIdText, ct);
        var now = clock.UtcNowSeconds();

        if (await holds.TryCancelAsync(hold.Id, studentId, now, ct))
            return;

        if (hold.StudentId != studentId)
            throw AppException.Forbidden(ErrorCodes.NotYourHold,
                "This application belongs to a different student.");

        var current = await FindHoldAsync(holdIdText, ct);
        var status = StatusAt(current, now);

        if (status == HoldStatus.CANCELLED)                      // already cancelled → nothing to do
            return;

        if (status == HoldStatus.CONFIRMED)
            throw AppException.Conflict(ErrorCodes.CannotCancelConfirmed,
                "A paid admission cannot be cancelled.");

        throw AppException.Conflict(ErrorCodes.HoldExpired, "This application has already expired.");
    }

    // ---------- My applications ----------
    public async Task<IReadOnlyList<MyHoldResponse>> ListMineAsync(string studentId, CancellationToken ct)
    {
        var now = clock.UtcNowSeconds();
        var list = await holds.ListByStudentAsync(studentId, ct);

        return list.Select(h => new MyHoldResponse(
            h.Id, h.ClassId, h.Class.Name, StatusAt(h, now).ToString(), h.ExpiresAt)).ToList();
    }

    // ---------- Helpers ----------

    // A HELD hold counts as EXPIRED the moment its time is up, even before the DB row is updated.
    private static HoldStatus StatusAt(Hold hold, DateTime now) =>
        hold.Status == HoldStatus.HELD && hold.ExpiresAt <= now ? HoldStatus.EXPIRED : hold.Status;

    private async Task<Hold> FindHoldAsync(string holdIdText, CancellationToken ct)
    {
        if (!Guid.TryParse(holdIdText, out var holdId))
            throw AppException.NotFound(ErrorCodes.HoldNotFound, "This application was not found.");

        return await holds.GetByIdAsync(holdId, ct)
            ?? throw AppException.NotFound(ErrorCodes.HoldNotFound, "This application was not found.");
    }
}
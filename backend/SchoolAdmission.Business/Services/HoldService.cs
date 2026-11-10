using Microsoft.Extensions.Options;
using SchoolAdmission.Business.Contracts;
using SchoolAdmission.Business.Errors;
using SchoolAdmission.Business.Options;
using SchoolAdmission.Data.Entities;
using SchoolAdmission.Data.Repositories;

namespace SchoolAdmission.Business.Services;

public class HoldService : IHoldService
{
    private readonly IClassRepository _classRepository;
    private readonly IHoldRepository _holdRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _clock;
    private readonly HoldOptions _options;

    public HoldService
        (
            IClassRepository classRepository,
            IHoldRepository holdRepository,
            IUnitOfWork unitOfWork,
            TimeProvider clock,
            IOptions<HoldOptions> options
        )
    {
        _classRepository = classRepository;
        _holdRepository = holdRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _options = options.Value;
    }

    // ---------- Apply for a seat ----------
    public async Task<HoldCreatedResponse> CreateAsync(string classIdText, string studentId, CancellationToken ct)
    {
        Guid classId;
        if (!Guid.TryParse(classIdText, out classId))
        {
            throw AppException.NotFound(ErrorCodes.ClassNotFound, "This class does not exist.");
        }

        DateTime now = _clock.UtcNowSeconds();

        // Everything below runs in ONE database transaction.
        // If we leave this method without calling CommitAsync, the transaction is rolled back.
        await using ITransactionScope transaction = await _unitOfWork.BeginTransactionAsync(ct);

        // 1. Lock the class row. Other requests for THIS class wait here until we commit.
        SchoolClass? schoolClass = await _classRepository.GetForUpdateAsync(classId, ct);
        if (schoolClass == null)
        {
            throw AppException.NotFound(ErrorCodes.ClassNotFound, "This class does not exist.");
        }

        // 2. Mark this class's time-expired holds as EXPIRED.
        await _holdRepository.ExpireStaleForClassAsync(classId, now, ct);

        // 3. One active application per student per class.
        Guid? existingHoldId = await _holdRepository.FindActiveHoldIdAsync(classId, studentId, now, ct);
        if (existingHoldId != null)
        {
            throw AppException.Conflict(
                ErrorCodes.AlreadyApplied,
                "This student already has an active application for this class.",
                new { holdId = existingHoldId });   // the UI uses this to open the existing application
        }

        // 4. Is there a free seat?
        int usedSeats = await _holdRepository.CountActiveAsync(classId, now, ct);
        if (usedSeats >= schoolClass.TotalSeats)
        {
            throw AppException.Conflict(ErrorCodes.NoSeatsAvailable,
                "Sorry, all seats in this class are taken right now.");
        }

        // 5. Create the hold, save it, and commit (commit releases the lock).
        Hold hold = new Hold
        {
            Id = Guid.NewGuid(),
            ClassId = classId,
            StudentId = studentId,
            Status = HoldStatus.HELD,
            CreatedAt = now,
            ExpiresAt = now.AddSeconds(_options.HoldDurationSeconds)
        };

        _holdRepository.Add(hold);
        await _unitOfWork.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return new HoldCreatedResponse(hold.Id, classId, studentId, "HELD", hold.ExpiresAt, now);
    }

    // ---------- View one application ----------
    public async Task<HoldResponse> GetAsync(string holdIdText, string? studentId, CancellationToken ct)
    {
        Hold hold = await FindHoldAsync(holdIdText, ct);

        if (studentId != null && hold.StudentId != studentId)
        {
            throw AppException.Forbidden(ErrorCodes.NotYourHold,
                "This application belongs to a different student.");
        }

        DateTime now = _clock.UtcNowSeconds();
        HoldStatus status = StatusAt(hold, now);

        return new HoldResponse(hold.Id, hold.ClassId, hold.Class.Name, status.ToString(), hold.ExpiresAt, now);
    }

    // ---------- Pay (confirm) ----------
    public async Task<ConfirmResponse> ConfirmAsync(string holdIdText, string studentId, CancellationToken ct)
    {
        Hold hold = await FindHoldAsync(holdIdText, ct);
        DateTime now = _clock.UtcNowSeconds();

        // One UPDATE ... WHERE status = 'HELD' AND not expired. Only one call can ever change the row.
        bool confirmed = await _holdRepository.TryConfirmAsync(hold.Id, studentId, now, ct);
        if (confirmed)
        {
            return new ConfirmResponse(hold.Id, "CONFIRMED");
        }

        // Nothing changed. Find out why.
        if (hold.StudentId != studentId)
        {
            throw AppException.Forbidden(ErrorCodes.NotYourHold,
                "This application belongs to a different student.");
        }

        Hold current = await FindHoldAsync(holdIdText, ct);   // read again: it may have just changed
        HoldStatus status = StatusAt(current, now);

        if (status == HoldStatus.CONFIRMED)
        {
            // Already paid: give the same answer again (idempotent).
            return new ConfirmResponse(hold.Id, "CONFIRMED");
        }

        if (status == HoldStatus.CANCELLED)
        {
            throw AppException.Conflict(ErrorCodes.HoldCancelled, "This application was cancelled.");
        }

        throw AppException.Conflict(ErrorCodes.HoldExpired,
            "Time is up. This application expired before payment. Please apply again.");
    }

    // ---------- Cancel ----------
    public async Task CancelAsync(string holdIdText, string studentId, CancellationToken ct)
    {
        Hold hold = await FindHoldAsync(holdIdText, ct);
        DateTime now = _clock.UtcNowSeconds();

        bool cancelled = await _holdRepository.TryCancelAsync(hold.Id, studentId, now, ct);
        if (cancelled)
        {
            return;
        }

        // Nothing changed. Find out why.
        if (hold.StudentId != studentId)
        {
            throw AppException.Forbidden(ErrorCodes.NotYourHold,
                "This application belongs to a different student.");
        }

        Hold current = await FindHoldAsync(holdIdText, ct);
        HoldStatus status = StatusAt(current, now);

        if (status == HoldStatus.CANCELLED)
        {
            // Already cancelled: nothing to do.
            return;
        }

        if (status == HoldStatus.CONFIRMED)
        {
            throw AppException.Conflict(ErrorCodes.CannotCancelConfirmed,
                "A paid admission cannot be cancelled.");
        }

        throw AppException.Conflict(ErrorCodes.HoldExpired, "This application has already expired.");
    }

    // ---------- My applications ----------
    public async Task<IReadOnlyList<MyHoldResponse>> ListMineAsync(string studentId, CancellationToken ct)
    {
        DateTime now = _clock.UtcNowSeconds();
        IReadOnlyList<Hold> holds = await _holdRepository.ListByStudentAsync(studentId, ct);

        List<MyHoldResponse> result = new List<MyHoldResponse>();
        foreach (Hold hold in holds)
        {
            HoldStatus status = StatusAt(hold, now);
            result.Add(new MyHoldResponse(hold.Id, hold.ClassId, hold.Class.Name, status.ToString(), hold.ExpiresAt));
        }

        return result;
    }

    // ---------- Background sweeper ----------
    public async Task<int> ExpireStaleHoldsAsync(CancellationToken ct)
    {
        DateTime now = _clock.UtcNowSeconds();
        return await _holdRepository.ExpireAllStaleAsync(now, ct);
    }

    // ---------- Helpers ----------

    // A HELD hold counts as EXPIRED the moment its time is up, even before the database row is updated.
    private static HoldStatus StatusAt(Hold hold, DateTime now)
    {
        if (hold.Status == HoldStatus.HELD && hold.ExpiresAt <= now)
        {
            return HoldStatus.EXPIRED;
        }

        return hold.Status;
    }

    private async Task<Hold> FindHoldAsync(string holdIdText, CancellationToken ct)
    {
        Guid holdId;
        if (!Guid.TryParse(holdIdText, out holdId))
        {
            throw AppException.NotFound(ErrorCodes.HoldNotFound, "This application was not found.");
        }

        Hold? hold = await _holdRepository.GetByIdAsync(holdId, ct);
        if (hold == null)
        {
            throw AppException.NotFound(ErrorCodes.HoldNotFound, "This application was not found.");
        }

        return hold;
    }
}
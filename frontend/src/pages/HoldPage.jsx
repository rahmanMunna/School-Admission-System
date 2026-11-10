import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { api } from "../api";
import ErrorMessage from "../components/ErrorMessage";
import { usePolling } from "../hooks/usePolling";

// 125 → "02:05"
function formatTime(totalSeconds) {
  const minutes = String(Math.floor(totalSeconds / 60)).padStart(2, "0");
  const seconds = String(totalSeconds % 60).padStart(2, "0");
  return `${minutes}:${seconds}`;
}

export default function HoldPage({ studentId }) {
  const { holdId } = useParams();

  // Reload the application from the server every 5 seconds (catches changes made elsewhere)
  const { data: hold, error, refresh } = usePolling(
    () => api.getHold(holdId, studentId || undefined),
    5000,
    [holdId, studentId]
  );

  const [busy, setBusy] = useState(false);              // a Pay/Cancel request is running
  const [actionError, setActionError] = useState(null);
  const [message, setMessage] = useState("");

  // The clock: update "now" every second so the countdown moves
  const [now, setNow] = useState(Date.now());
  useEffect(() => {
    const timer = setInterval(() => setNow(Date.now()), 1000);
    return () => clearInterval(timer);
  }, []);

  // Seconds left until expiresAt (never below 0)
  const secondsLeft = hold
    ? Math.max(0, Math.floor((Date.parse(hold.expiresAt) - now) / 1000))
    : 0;

  // Time is up but the last server answer still said HELD → show EXPIRED right away
  const expiredByTime = hold?.status === "HELD" && secondsLeft === 0;
  const status = expiredByTime ? "EXPIRED" : hold?.status;

  // When the countdown hits 0, ask the server once to confirm
  useEffect(() => {
    if (expiredByTime) refresh();
  }, [expiredByTime, refresh]);

  async function handlePay() {
    setBusy(true);
    setActionError(null);
    try {
      await api.confirmHold(holdId, studentId);
      setMessage("Payment successful. Admission confirmed.");
    } catch (err) {
      setActionError(err);
    } finally {
      await refresh();
      setBusy(false);
    }
  }

  async function handleCancel() {
    if (!window.confirm("Cancel this application? The seat will be released.")) return;
    setBusy(true);
    setActionError(null);
    try {
      await api.cancelHold(holdId, studentId);
      setMessage("Application cancelled. The seat has been released.");
    } catch (err) {
      setActionError(err);
    } finally {
      await refresh();
      setBusy(false);
    }
  }

  // Could not load at all (wrong link, other student's application, server down)
  if (!hold) {
    return (
      <div>
        {error ? <ErrorMessage error={error} /> : <p>Loading…</p>}
        <Link to="/">Back to classes</Link>
      </div>
    );
  }

  return (
    <div>
      <div className="card">
        <h2>{hold.className}</h2>
        <p>Status: <span className={`badge ${status}`}>{status}</span></p>

        {status === "HELD" && (
          <p>Time left to pay: <b style={{ fontSize: 22 }}>{formatTime(secondsLeft)}</b></p>
        )}
        {status === "EXPIRED" && (
          <p className="error">Time is up. The seat was released. You can apply again from the class list.</p>
        )}
        {status === "CONFIRMED" && <p className="success">Admission confirmed.</p>}
        {status === "CANCELLED" && <p className="muted">This application was cancelled.</p>}

        {message && <p className="success">{message}</p>}
        <ErrorMessage error={actionError} />
        {error && <ErrorMessage error={error} />}

        {(status === "HELD" || status === "EXPIRED") && (
          <div style={{ display: "flex", gap: 10 }}>
            {/* Pay is disabled the moment the countdown reaches 00:00 */}
            <button onClick={handlePay} disabled={status !== "HELD" || busy || !studentId}>
              {busy ? "Please wait…" : "Pay admission fee"}
            </button>
            {status === "HELD" && (
              <button className="secondary" onClick={handleCancel} disabled={busy || !studentId}>
                Cancel application
              </button>
            )}
          </div>
        )}

        {!studentId && (
          <p className="muted">Enter the student's birth registration number above to pay or cancel.</p>
        )}
      </div>

      <p>
        <Link to="/">Back to classes</Link> · <Link to="/my">My applications</Link>
      </p>
    </div>
  );
}
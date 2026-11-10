import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { api } from "../api";
import ErrorMessage from "../components/ErrorMessage";
import { usePolling } from "../hooks/usePolling";

export default function ClassListPage({ studentId }) {
  const navigate = useNavigate();
  const { data: classes, error, refresh } = usePolling(() => api.getClasses(), 3000);

  const [applyingId, setApplyingId] = useState(null);   // which class is being applied for
  const [applyError, setApplyError] = useState(null);

  async function handleApply(classId) {
    setApplyError(null);
    setApplyingId(classId);
    try {
      const hold = await api.applyForSeat(classId, studentId);
      navigate(`/holds/${hold.holdId}`);               // go to the application page
    } catch (err) {
      // Already applied? Just open the existing application.
      if (err.code === "ALREADY_APPLIED" && err.details?.holdId) {
        navigate(`/holds/${err.details.holdId}`);
        return;
      }
      setApplyError(err);                              // e.g. "all seats are taken"
      refresh();                                       // show the latest counts
    } finally {
      setApplyingId(null);
    }
  }

  return (
    <div>
      <h2>Classes</h2>

      {!studentId && (
        <p className="muted">Enter the student's birth registration number above to apply.</p>
      )}

      <ErrorMessage error={applyError} />

      {/* Server problem: show the message but keep the last data on screen */}
      {error && <ErrorMessage error={error} />}

      {classes === null && !error && <p>Loading…</p>}

      {classes?.length === 0 && (
        <p>No classes yet. The school admin can create them on the <b>School admin</b> page.</p>
      )}

      {classes?.length > 0 && (
        <table>
          <thead>
            <tr>
              <th>Class</th>
              <th>Total</th>
              <th>Held</th>
              <th>Confirmed</th>
              <th>Available</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {classes.map((c) => {
              const isFull = c.availableSeats === 0;
              const isApplying = applyingId === c.id;
              return (
                <tr key={c.id}>
                  <td>{c.name}</td>
                  <td>{c.totalSeats}</td>
                  <td>{c.heldSeats}</td>
                  <td>{c.confirmedSeats}</td>
                  <td><b>{c.availableSeats}</b></td>
                  <td>
                    <button
                      onClick={() => handleApply(c.id)}
                      disabled={!studentId || isFull || applyingId !== null}
                    >
                      {isApplying ? "Applying…" : isFull ? "Full" : "Apply"}
                    </button>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      )}

      <p className="muted">Seat counts update automatically every 3 seconds.</p>
    </div>
  );
}
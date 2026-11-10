import { Link } from "react-router-dom";
import { api } from "../api";
import ErrorMessage from "../components/ErrorMessage";
import { usePolling } from "../hooks/usePolling";

export default function MyHoldsPage({ studentId }) {
  // Reload every 5 seconds so statuses (e.g. EXPIRED) stay up to date
  const { data: holds, error } = usePolling(
    () => (studentId ? api.getMyHolds(studentId) : Promise.resolve([])),
    5000,
    [studentId]
  );

  if (!studentId) {
    return (
      <div>
        <h2>My applications</h2>
        <p className="muted">Enter the student's birth registration number above to see the applications.</p>
      </div>
    );
  }

  return (
    <div>
      <h2>My applications</h2>

      {error && <ErrorMessage error={error} />}
      {holds === null && !error && <p>Loading…</p>}

      {holds?.length === 0 && (
        <p>No applications yet. <Link to="/">Apply for a class</Link>.</p>
      )}

      {holds?.length > 0 && (
        <table>
          <thead>
            <tr>
              <th>Class</th>
              <th>Status</th>
              <th>Expires at</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {holds.map((h) => (
              <tr key={h.holdId}>
                <td>{h.className}</td>
                <td><span className={`badge ${h.status}`}>{h.status}</span></td>
                <td>{new Date(h.expiresAt).toLocaleString()}</td>
                <td><Link to={`/holds/${h.holdId}`}>Open</Link></td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
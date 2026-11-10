import { useState } from "react";
import { api } from "../api";
import ErrorMessage from "../components/ErrorMessage";
import { usePolling } from "../hooks/usePolling";

export default function AdminPage() {
  const { data: classes, error: listError, refresh } = usePolling(() => api.getClasses(), 3000);

  const [name, setName] = useState("");
  const [seats, setSeats] = useState("");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const [message, setMessage] = useState("");

  async function handleSubmit(event) {
    event.preventDefault();
    setError(null);
    setMessage("");

    // Quick checks in the browser (the backend checks again)
    const totalSeats = Number(seats);
    if (!name.trim()) {
      setError(new Error("Please enter a class name."));
      return;
    }
    if (!Number.isInteger(totalSeats) || totalSeats < 1) {
      setError(new Error("Seat count must be a whole number of 1 or more."));
      return;
    }

    setSaving(true);
    try {
      const created = await api.createClass(name.trim(), totalSeats);
      setMessage(`Class "${created.name}" created with ${created.totalSeats} seats.`);
      setName("");
      setSeats("");
      refresh();                                   // show it in the list right away
    } catch (err) {
      setError(err);
    } finally {
      setSaving(false);
    }
  }

  return (
    <div>
      <h2>School admin</h2>
      <p className="muted">Admin area: no login is required in this version.</p>

      <form className="card" onSubmit={handleSubmit}>
        <h3>Create a class</h3>
        <p className="muted">Each shift is a separate class, e.g. "Class 6 – Morning Shift".</p>

        <div style={{ display: "flex", gap: 10, flexWrap: "wrap", alignItems: "center" }}>
          <input
            placeholder="Class name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            style={{ width: 280 }}
          />
          <input
            type="number"
            min="1"
            step="1"
            placeholder="Seats"
            value={seats}
            onChange={(e) => setSeats(e.target.value)}
            style={{ width: 110 }}
          />
          <button type="submit" disabled={saving}>
            {saving ? "Creating…" : "Create class"}
          </button>
        </div>

        <ErrorMessage error={error} />
        {message && <p className="success">{message}</p>}
      </form>

      <h3>All classes</h3>
      {listError && <ErrorMessage error={listError} />}
      {classes?.length === 0 && <p className="muted">No classes yet.</p>}
      {classes?.length > 0 && (
        <table>
          <thead>
            <tr>
              <th>Class</th>
              <th>Total</th>
              <th>Held</th>
              <th>Confirmed</th>
              <th>Available</th>
            </tr>
          </thead>
          <tbody>
            {classes.map((c) => (
              <tr key={c.id}>
                <td>{c.name}</td>
                <td>{c.totalSeats}</td>
                <td>{c.heldSeats}</td>
                <td>{c.confirmedSeats}</td>
                <td><b>{c.availableSeats}</b></td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
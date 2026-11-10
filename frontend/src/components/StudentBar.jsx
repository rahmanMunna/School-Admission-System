import { useState } from "react";
import { isValidStudentId, normalizeStudentId } from "../studentId";

// Two states:
//  - no student yet → input + Continue
//  - student set    → shows the number read-only + "Change student"
export default function StudentBar({ studentId, onChange }) {
  const [text, setText] = useState("");
  const [error, setError] = useState("");

  if (studentId) {
    return (
      <div className="card student-bar">
        <span>Student: <b>{studentId}</b> ✓</span>
        <button
          className="secondary"
          onClick={() => {
            if (window.confirm("Switch to a different student?")) onChange("");
          }}
        >
          Change student
        </button>
      </div>
    );
  }

  function handleSubmit(event) {
    event.preventDefault();
    const id = normalizeStudentId(text);

    if (!isValidStudentId(id)) {
      setError("Birth registration number must be exactly 17 digits.");
      return;
    }

    setError("");
    setText("");
    onChange(id);
  }

  return (
    <form className="card student-bar" onSubmit={handleSubmit}>
      <label htmlFor="studentId">Student's birth registration number</label>
      <input
        id="studentId"
        type="text"            /* NOT type="number": that drops leading zeros */
        inputMode="numeric"    /* shows the number keyboard on phones */
        autoComplete="off"
        placeholder="17 digits"
        value={text}
        onChange={(e) => { setText(e.target.value); setError(""); }}
      />
      <button type="submit">Continue</button>
      {error && <div className="error">{error}</div>}
    </form>
  );
}
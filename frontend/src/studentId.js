// Everything about the student ID in one place.
const STORAGE_KEY = "admission.studentId";
const BANGLA_DIGITS = "০১২৩৪৫৬৭৮৯";

// "২০১৫ 2691-2345..." → "20152691234..."
// Converts Bangla digits to 0-9 and removes spaces and dashes.
export function normalizeStudentId(text) {
  return text
    .replace(/[০-৯]/g, (d) => String(BANGLA_DIGITS.indexOf(d)))
    .replace(/[\s-]/g, "");
}

// Exactly 17 digits. Always kept as a STRING: 17 digits is too big for a JS number.
export function isValidStudentId(id) {
  return /^[0-9]{17}$/.test(id);
}

// Remember the ID in the browser so a refresh doesn't lose it.
export function loadStudentId() {
  try {
    const id = localStorage.getItem(STORAGE_KEY);
    return id && isValidStudentId(id) ? id : "";
  } catch {
    return "";
  }
}

export function saveStudentId(id) {
  try { localStorage.setItem(STORAGE_KEY, id); } catch { /* storage unavailable */ }
}

export function clearStudentId() {
  try { localStorage.removeItem(STORAGE_KEY); } catch { /* storage unavailable */ }
}
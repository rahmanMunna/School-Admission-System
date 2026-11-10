import { API_BASE_URL } from "./config";

// Error with the backend's code and a message we can show to the guardian.
export class ApiError extends Error {
  constructor(status, code, message, details) {
    super(message);
    this.status = status;    // 0 = server not reachable
    this.code = code;        // e.g. "NO_SEATS_AVAILABLE"
    this.details = details;  // e.g. { holdId } for ALREADY_APPLIED
  }
}

// One function for all requests: adds headers, turns every failure into an ApiError.
async function request(path, { method = "GET", body, studentId } = {}) {
  const headers = {};
  if (body !== undefined) headers["Content-Type"] = "application/json";
  if (studentId) headers["X-Student-Id"] = studentId;

  let response;
  try {
    response = await fetch(API_BASE_URL + path, {
      method,
      headers,
      body: body !== undefined ? JSON.stringify(body) : undefined,
    });
  } catch {
    // fetch only throws when the server can't be reached at all
    throw new ApiError(0, "NETWORK", "Cannot reach the server. Please check your connection. We will keep trying.");
  }

  if (response.status === 204) return null;                // DELETE has no body

  const data = await response.json().catch(() => null);

  if (!response.ok) {
    throw new ApiError(
      response.status,
      data?.error ?? `HTTP_${response.status}`,
      data?.message ?? `Something went wrong (${response.status}). Please try again.`,
      data?.details
    );
  }

  return data;
}

// The 8 endpoints from the brief
export const api = {
  getClasses:   ()                    => request("/classes"),
  getClass:     (classId)             => request(`/classes/${classId}`),
  createClass:  (name, totalSeats)    => request("/classes", { method: "POST", body: { name, totalSeats } }),
  applyForSeat: (classId, studentId)  => request(`/classes/${classId}/holds`, { method: "POST", studentId }),
  getHold:      (holdId, studentId)   => request(`/holds/${holdId}`, { studentId }),
  confirmHold:  (holdId, studentId)   => request(`/holds/${holdId}/confirm`, { method: "POST", studentId }),
  cancelHold:   (holdId, studentId)   => request(`/holds/${holdId}`, { method: "DELETE", studentId }),
  getMyHolds:   (studentId)           => request("/me/holds", { studentId }),
};
import { afterEach, describe, expect, it, vi } from "vitest";
import { api } from "../api";

describe("api errors", () => {
  afterEach(() => vi.unstubAllGlobals());           // put the real fetch back after each test

  it("server not reachable → NETWORK error with a friendly message", async () => {
    vi.stubGlobal("fetch", vi.fn().mockRejectedValue(new TypeError("Failed to fetch")));

    await expect(api.getClasses()).rejects.toMatchObject({
      status: 0,
      code: "NETWORK",
      message: expect.stringMatching(/cannot reach the server/i),
    });
  });

  it("409 from the backend → its error code and message", async () => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue({
      ok: false,
      status: 409,
      json: async () => ({ error: "NO_SEATS_AVAILABLE", message: "Sorry, all seats in this class are taken right now." }),
    }));

    await expect(api.applyForSeat("class-1", "20152691234567890")).rejects.toMatchObject({
      status: 409,
      code: "NO_SEATS_AVAILABLE",
      message: "Sorry, all seats in this class are taken right now.",
    });
  });
});
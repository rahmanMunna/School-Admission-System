import { describe, expect, it } from "vitest";
import { isValidStudentId, normalizeStudentId } from "../studentId";

describe("studentId rules", () => {
  it("removes spaces and dashes", () => {
    expect(normalizeStudentId("2015 2691-2345 67890")).toBe("20152691234567890");
  });

  it("converts Bangla digits to 0-9", () => {
    expect(normalizeStudentId("২০১৫২৬৯১২৩৪৫৬৭৮৯০")).toBe("20152691234567890");
  });

  it("accepts exactly 17 digits", () => {
    expect(isValidStudentId("20152691234567890")).toBe(true);
  });

  it("rejects 16 digits, 18 digits and letters", () => {
    expect(isValidStudentId("2015269123456789")).toBe(false);
    expect(isValidStudentId("201526912345678901")).toBe(false);
    expect(isValidStudentId("2015269123456789A")).toBe(false);
  });

  it("keeps the ID as text (leading zeros are not lost)", () => {
    expect(normalizeStudentId("01234567890123456")).toBe("01234567890123456");
  });
});
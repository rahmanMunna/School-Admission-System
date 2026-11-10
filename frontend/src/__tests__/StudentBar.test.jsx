import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import StudentBar from "../components/StudentBar";

describe("StudentBar", () => {
  it("shows an error for a number that is not 17 digits", async () => {
    const onChange = vi.fn();                         // a fake function that records calls
    render(<StudentBar studentId="" onChange={onChange} />);

    await userEvent.type(screen.getByLabelText(/birth registration number/i), "123");
    await userEvent.click(screen.getByRole("button", { name: "Continue" }));

    expect(screen.getByText(/exactly 17 digits/i)).toBeInTheDocument();
    expect(onChange).not.toHaveBeenCalled();
  });

  it("accepts a valid number with spaces and passes it on cleaned", async () => {
    const onChange = vi.fn();
    render(<StudentBar studentId="" onChange={onChange} />);

    await userEvent.type(screen.getByLabelText(/birth registration number/i), "2015 2691 2345 67890");
    await userEvent.click(screen.getByRole("button", { name: "Continue" }));

    expect(onChange).toHaveBeenCalledWith("20152691234567890");
  });

  it("shows the locked student with a Change student button", () => {
    render(<StudentBar studentId="20152691234567890" onChange={vi.fn()} />);

    expect(screen.getByText("20152691234567890")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Change student" })).toBeInTheDocument();
  });
});
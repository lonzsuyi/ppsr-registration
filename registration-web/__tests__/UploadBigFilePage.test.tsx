import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import '@testing-library/jest-dom';
import Home from "../src/app/upload/bigfile/page";

describe("PPSR Big File Batch Upload Registration", () => {
  it("should show heading and file input", () => {
    render(<Home />);
    expect(screen.getByText(/PPSR Big File Batch Upload Registration/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/upload/i)).toBeInTheDocument();
  });

  it("should show error message if no file is selected", async () => {
    render(<Home />);
    const uploadBtn = screen.getByRole("button", { name: /Upload/i });
    fireEvent.click(uploadBtn);

    await waitFor(() => {
      expect(screen.getByText(/Please select a file/i)).toBeInTheDocument();
    });
  });

  it("should disable upload button while loading", async () => {
    render(<Home />);
    const fakeFile = new File(["col1,col2"], "data.csv", { type: "text/csv" });
    const input = screen.getByLabelText(/upload/i) as HTMLInputElement;

    fireEvent.change(input, { target: { files: [fakeFile] } });

    const uploadBtn = screen.getByRole("button", { name: /Upload/i });
    expect(uploadBtn).not.toBeDisabled();
  });
});
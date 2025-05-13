import { test, expect } from "@playwright/test";
import path from "path";

test("Upload CSV and see summary result (non-background)", async ({ page }) => {
  // open upload page
  await page.goto("http://localhost:3000/upload/file");

  // mock file selection (ensure this file exists)
  const filePath = path.resolve(__dirname, "sample.csv");
  await page.setInputFiles('input[type="file"]', filePath);

  // click upload button
  await page.getByTestId("upload-submit-btn").click();

  // wait for success message
  await expect(
    page.getByText(/Upload successful: \d+ records processed/)
  ).toBeVisible({ timeout: 15000 });

  // check each field exists
  await expect(page.getByText(/Submitted Total:/)).toBeVisible();
  await expect(page.getByText(/Invalid Records:/)).toBeVisible();
  await expect(page.getByText(/Processed Successfully:/)).toBeVisible();
  await expect(page.getByText(/Added Records:/)).toBeVisible();
  await expect(page.getByText(/Updated Records:/)).toBeVisible();
});
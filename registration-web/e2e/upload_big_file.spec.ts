import { test, expect } from "@playwright/test";
import path from "path";

test("Upload valid CSV and show result summary", async ({ page }) => {
  // access upload page
  await page.goto("http://localhost:3000/upload/bigfile");

  // select test file (put in project root e2e/files/sample.csv)
  const filePath = path.resolve(__dirname, "sample.csv");
  const fileChooserPromise = page.waitForEvent("filechooser");

  // trigger file upload dialog
  await page.getByTestId("upload-submit-btn").click();
  const fileChooser = await fileChooserPromise;
  await fileChooser.setFiles(filePath);

  // click upload button
  await page.getByTestId("upload-button").click();

  // check upload status is in Processing stage
  await expect(page.locator("text=Status:")).toBeVisible({ timeout: 30000 });

  // wait for status to become Completed (max wait 60 seconds)
  await page.waitForFunction(
    () => {
      const status = document.querySelector("span.text-green-600");
      return status?.textContent?.includes("Completed");
    },
    null,
    { timeout: 60000 }
  );

  // check final result is shown
  await expect(page.locator("text=Submitted:")).toBeVisible();
  await expect(page.locator("text=Processed:")).toBeVisible();

  // check history is shown
  await expect(page.locator("text=Upload History")).toBeVisible();
});
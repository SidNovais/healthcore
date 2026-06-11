import { test, expect } from '@playwright/test';

const PHYSICIAN_EMAIL = 'physician@hclis.local';
const PHYSICIAN_PASSWORD = 'Admin1234!';

async function loginAsPhysician(page: import('@playwright/test').Page) {
  await page.goto('/login');
  await page.getByLabel('Email').fill(PHYSICIAN_EMAIL);
  await page.getByLabel('Password').fill(PHYSICIAN_PASSWORD);
  await page.getByRole('button', { name: /sign in/i }).click();
  await expect(page).toHaveURL('/worklist', { timeout: 10_000 });
}

test.describe('Doctor Worklist', () => {
  test('Physician sees worklist on login', async ({ page }) => {
    await loginAsPhysician(page);
    await expect(page.getByTestId('worklist-title')).toBeVisible({ timeout: 5_000 });
  });

  test('Physician cannot access /waiting-room (role guard)', async ({ page }) => {
    await loginAsPhysician(page);
    await page.goto('/waiting-room');
    await expect(page).toHaveURL('/unauthorized', { timeout: 5_000 });
  });

  // Requires a WorklistItem ready for signing (completed sample analysis).
  // Depends on the full TestOrders→SampleCollection→LabAnalysis pipeline running via RabbitMQ.
  // Run with the complete event infrastructure to enable this test.
  test.fixme('full sign-report workflow: refresh → row visible → click row → detail panel → sign report → confirmation', async ({ page }) => {
    await loginAsPhysician(page);

    // Refresh to load items (requires seed data with a completed worklist item)
    await page.getByTestId('refresh-btn').click();
    await expect(page.getByTestId('worklist-row').first()).toBeVisible({ timeout: 10_000 });

    // Assert patient name is not displayed as UUID
    const patientCell = page.getByTestId('patient-name-cell').first();
    await expect(patientCell).not.toContainText(/^[0-9a-f]{8}-/i);

    // Click a row to open detail panel
    await page.getByTestId('worklist-row').first().click();
    await expect(page.getByTestId('worklist-item-detail')).toBeVisible({ timeout: 3_000 });

    // Assert patient name in detail panel is not displayed as UUID
    await expect(page.getByTestId('patient-name')).not.toContainText(/^[0-9a-f]{8}-/i);

    // Fill signature and sign report
    await page.getByTestId('signature-input').fill('Dr. House');
    await page.getByTestId('sign-report-btn').click();
    await page.waitForResponse(resp => resp.url().includes('sign') && resp.status() === 201);

    // Confirmation element is visible
    await expect(page.getByTestId('sign-report-confirmation')).toBeVisible({ timeout: 5_000 });
  });
});

import { test, expect } from '@playwright/test';
import { loginAsPhysician } from './fixtures/auth';

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

  test('worklist row shows the requesting physician name, never a raw id', async ({ page }) => {
    await page.route(/\/api\/v1\/worklist-items(\?.*)?$/, async route => {
      if (route.request().method() === 'GET') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify([
            {
              id: '00000000-0000-0000-0000-0000000000b1',
              sampleBarcode: 'BC-E2E-001',
              examCode: 'HGB',
              patientId: '00000000-0000-0000-0000-000000000001',
              patientName: 'Maria Silva',
              requestedByName: 'Ana Lima',
              status: 'Pending',
              createdAt: new Date().toISOString(),
            },
            {
              id: '00000000-0000-0000-0000-0000000000b2',
              sampleBarcode: 'BC-E2E-002',
              examCode: 'WBC',
              patientId: '00000000-0000-0000-0000-000000000001',
              patientName: 'Maria Silva',
              requestedByName: null,
              status: 'Pending',
              createdAt: new Date().toISOString(),
            },
          ]),
        });
        return;
      }
      await route.continue();
    });

    await loginAsPhysician(page);
    await page.reload();
    await expect(page.getByTestId('worklist-row').first()).toBeVisible({ timeout: 10_000 });

    const cells = page.getByTestId('requested-by-cell');
    await expect(cells.first()).toHaveText('Ana Lima');
    await expect(cells.nth(1)).toHaveText('Unknown physician');
    await expect(cells.first()).not.toContainText(/^[0-9a-f]{8}-/i);
  });

  // Requires a WorklistItem ready for signing (completed sample analysis).
  // Depends on the full TestOrders→SampleCollection→LabAnalysis pipeline running via RabbitMQ.
  // Run with the complete event infrastructure to enable this test.
  test.fixme('full sign-report workflow: refresh → row visible → click row → detail panel → sign report → confirmation', async ({ page }) => {
    await loginAsPhysician(page);

    // The list loads on navigation and updates live; reload to pull seeded items.
    await page.reload();
    await expect(page.getByTestId('worklist-row').first()).toBeVisible({ timeout: 10_000 });

    // Assert patient name is not displayed as UUID
    const patientCell = page.getByTestId('patient-name-cell').first();
    await expect(patientCell).not.toContainText(/^[0-9a-f]{8}-/i);

    // Click a row to open detail panel
    await page.getByTestId('worklist-row').first().click();
    await expect(page.getByTestId('worklist-item-detail')).toBeVisible({ timeout: 3_000 });

    // Assert patient name in detail panel is not displayed as UUID
    await expect(page.getByTestId('patient-name')).not.toContainText(/^[0-9a-f]{8}-/i);

    // The requesting physician is carried forward from the order, never as a raw id.
    await expect(page.getByTestId('requested-by')).toBeVisible();
    await expect(page.getByTestId('requested-by')).not.toContainText(/^[0-9a-f]{8}-/i);

    // Fill signature and sign report
    await page.getByTestId('signature-input').fill('Dr. House');
    await page.getByTestId('sign-report-btn').click();
    await page.waitForResponse(resp => resp.url().includes('sign') && resp.status() === 201);

    // Confirmation element is visible
    await expect(page.getByTestId('sign-report-confirmation')).toBeVisible({ timeout: 5_000 });
  });

  // Same seed-data dependency as the sign-report workflow above.
  test.fixme('row action menu View opens the detail panel', async ({ page }) => {
    await loginAsPhysician(page);

    await page.getByTestId('refresh-btn').click();
    await expect(page.getByTestId('worklist-row').first()).toBeVisible({ timeout: 10_000 });

    // Opening the row action menu must not itself select the row's detail panel.
    await page.getByTestId('worklist-actions-trigger').first().click();
    await expect(page.getByTestId('worklist-item-detail')).toBeHidden();

    await page.getByTestId('worklist-action-view').first().click();
    await expect(page.getByTestId('worklist-item-detail')).toBeVisible({ timeout: 3_000 });
  });
});

import { test, expect } from '@playwright/test';
import { loginAsPhysician, loginAsReceptionist } from './fixtures/auth';

test.describe('Patient Picker in New Order', () => {
  test.beforeEach(async ({ context }) => {
    await context.clearCookies();
  });

  test('Receptionist selects patient via typeahead and creates order — exam section visible', async ({ page }) => {
    await loginAsReceptionist(page);

    // Register a unique patient so we have a known name + document ID to search for
    const uniqueName = `PickerPatient-${Date.now()}`;
    const documentId = `DOC-PKR-${Date.now()}`;

    await page.goto('/patients/new');
    await page.getByTestId('patient-full-name-input').fill(uniqueName);
    await page.getByTestId('patient-dob-input').fill('1985-06-15');
    await page.getByTestId('patient-document-id-input').fill(documentId);

    await Promise.all([
      page.waitForResponse(r =>
        r.url().includes('/api/v1/patients') &&
        r.request().method() === 'POST' &&
        r.status() === 201,
      ),
      page.getByTestId('patient-form-submit-btn').click(),
    ]);

    await expect(page).toHaveURL(
      /\/patients\/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/,
      { timeout: 10_000 },
    );
    await expect(page.getByTestId('patient-status-badge')).toBeVisible({ timeout: 15_000 });

    // Navigate to new order page
    await page.goto('/orders/new');

    // Submit is disabled before a patient is selected
    await expect(page.getByTestId('create-order-submit-btn')).toBeDisabled();

    // Search by partial name in the picker
    await page.getByTestId('patient-picker-input').fill(uniqueName.slice(0, 12));

    // Wait for search API response (debounce fires after 300 ms)
    await page.waitForResponse(r =>
      r.url().includes('/api/v1/patients') &&
      r.request().method() === 'GET' &&
      r.status() === 200,
    );

    // Results dropdown visible. The partial-name query matches every PickerPatient-*
    // row (including any left by earlier runs), so target OUR patient by its full unique
    // name rather than assuming it sorts first.
    await expect(page.getByTestId('patient-picker-results')).toBeVisible({ timeout: 5_000 });
    const ourResult = page.getByTestId('patient-picker-result-item').filter({ hasText: uniqueName });
    await expect(ourResult).toBeVisible({ timeout: 5_000 });

    // Select the patient
    await ourResult.click();

    // Dropdown gone; selection card appears with name and document ID
    await expect(page.getByTestId('patient-picker-results')).not.toBeVisible();
    await expect(page.getByTestId('patient-picker-selected-card')).toBeVisible({ timeout: 5_000 });
    await expect(page.getByTestId('patient-picker-selected-card')).toContainText(uniqueName);
    await expect(page.getByTestId('patient-picker-selected-card')).toContainText(documentId);

    // Submit is now enabled
    await expect(page.getByTestId('create-order-submit-btn')).toBeEnabled();

    // Create the order
    await Promise.all([
      page.waitForResponse(r =>
        r.url().includes('/api/v1/orders') &&
        r.request().method() === 'POST' &&
        r.status() === 201,
      ),
      page.getByTestId('create-order-submit-btn').click(),
    ]);

    // Exam section appears after order is created
    await expect(page.getByTestId('exam-section')).toBeVisible({ timeout: 5_000 });
  });

  test('Receptionist can clear patient selection — submit becomes disabled again', async ({ page }) => {
    await loginAsReceptionist(page);

    // Register a patient to search for
    const uniqueName = `PickerClear-${Date.now()}`;

    await page.goto('/patients/new');
    await page.getByTestId('patient-full-name-input').fill(uniqueName);
    await page.getByTestId('patient-dob-input').fill('1990-03-10');

    await Promise.all([
      page.waitForResponse(r =>
        r.url().includes('/api/v1/patients') &&
        r.request().method() === 'POST' &&
        r.status() === 201,
      ),
      page.getByTestId('patient-form-submit-btn').click(),
    ]);

    await expect(page).toHaveURL(
      /\/patients\/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/,
      { timeout: 10_000 },
    );
    await expect(page.getByTestId('patient-status-badge')).toBeVisible({ timeout: 15_000 });

    await page.goto('/orders/new');

    // Search and select a patient
    await page.getByTestId('patient-picker-input').fill(uniqueName.slice(0, 12));
    await page.waitForResponse(r =>
      r.url().includes('/api/v1/patients') &&
      r.request().method() === 'GET' &&
      r.status() === 200,
    );
    await expect(page.getByTestId('patient-picker-result-item').first()).toBeVisible({ timeout: 5_000 });
    await page.getByTestId('patient-picker-result-item').first().click();

    await expect(page.getByTestId('patient-picker-selected-card')).toBeVisible({ timeout: 5_000 });
    await expect(page.getByTestId('create-order-submit-btn')).toBeEnabled();

    // Click the clear button
    await page.getByTestId('patient-picker-clear-btn').click();

    // Card gone; submit disabled; search input back
    await expect(page.getByTestId('patient-picker-selected-card')).not.toBeVisible();
    await expect(page.getByTestId('create-order-submit-btn')).toBeDisabled();
    await expect(page.getByTestId('patient-picker-input')).toBeVisible();
  });

  test('Physician is redirected to /unauthorized when accessing /orders/new', async ({ page }) => {
    await loginAsPhysician(page);
    await page.goto('/orders/new');
    await expect(page).toHaveURL('/unauthorized', { timeout: 5_000 });
  });
});

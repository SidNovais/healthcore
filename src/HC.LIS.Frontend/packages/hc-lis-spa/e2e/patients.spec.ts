import { test, expect } from '@playwright/test';
import {
  loginAsITAdmin,
  loginAsLabTechnician,
  loginAsPhysician,
  loginAsReceptionist,
} from './fixtures/auth';

test.describe('Patient Management', () => {
  test.beforeEach(async ({ context }) => {
    await context.clearCookies();
  });

  test('Receptionist: full register + edit workflow', async ({ page }) => {
    await loginAsReceptionist(page);

    const uniqueName = `TestPatient-${Date.now()}`;

    await page.goto('/patients');

    // Search for unique name — expect no results yet
    await page.getByTestId('patient-search-input').fill(uniqueName);
    await expect(page.getByTestId('patient-search-empty-state')).toBeVisible({ timeout: 10_000 });

    // Navigate to register form
    await page.getByTestId('register-patient-btn').click();
    await expect(page).toHaveURL('/patients/new', { timeout: 5_000 });

    // Fill form and submit; wait for POST 201
    await page.getByTestId('patient-full-name-input').fill(uniqueName);
    await page.getByTestId('patient-dob-input').fill('1990-01-15');

    await Promise.all([
      page.waitForResponse(r =>
        r.url().includes('/api/v1/patients') &&
        r.request().method() === 'POST' &&
        r.status() === 201),
      page.getByTestId('patient-form-submit-btn').click(),
    ]);

    // Wait for router to navigate to the detail page, then for loadDetails() to render
    await expect(page).toHaveURL(
      /\/patients\/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/,
      { timeout: 10_000 },
    );
    // patient-status-badge is inside @if(patientsService.patient()) — only visible once GET resolves
    await expect(page.getByTestId('patient-status-badge')).toBeVisible({ timeout: 15_000 });

    // Verify registered name is displayed. Scoped to the heading: Phase 14's breadcrumb
    // trails "Patients / {name}", so a bare getByText matches both it and the h1.
    await expect(page.getByRole('heading', { name: uniqueName })).toBeVisible({ timeout: 5_000 });

    // Open inline edit form
    await page.getByTestId('patient-edit-btn').click();

    // Change full name and submit; wait for PUT 204
    const updatedName = `${uniqueName}-edited`;
    await page.getByTestId('patient-full-name-input').clear();
    await page.getByTestId('patient-full-name-input').fill(updatedName);

    await Promise.all([
      page.waitForResponse(r =>
        r.url().includes('/api/v1/patients/') &&
        r.request().method() === 'PUT' &&
        r.status() === 204),
      page.getByTestId('patient-form-submit-btn').click(),
    ]);

    // onFormSubmit reloads details; wait for the badge to re-render with fresh data
    await expect(page.getByTestId('patient-status-badge')).toBeVisible({ timeout: 10_000 });

    // Verify updated name is displayed (heading-scoped, as above).
    await expect(page.getByRole('heading', { name: updatedName })).toBeVisible({ timeout: 5_000 });
  });

  test('Receptionist: row action menu View opens patient detail slide-over', async ({ page }) => {
    await loginAsReceptionist(page);

    const uniqueName = `RowActionPatient-${Date.now()}`;

    // Register a patient so the search table has a row to act on
    await page.goto('/patients/new');
    await page.getByTestId('patient-full-name-input').fill(uniqueName);
    await page.getByTestId('patient-dob-input').fill('1991-04-10');
    await Promise.all([
      page.waitForResponse(r =>
        r.url().includes('/api/v1/patients') &&
        r.request().method() === 'POST' &&
        r.status() === 201),
      page.getByTestId('patient-form-submit-btn').click(),
    ]);
    await expect(page.getByTestId('patient-status-badge')).toBeVisible({ timeout: 15_000 });

    await page.goto('/patients');
    await page.getByTestId('patient-search-input').fill(uniqueName);
    await expect(page.getByTestId('patient-row').first()).toBeVisible({ timeout: 10_000 });

    // Opening the row action menu must not navigate the (clickable) row.
    await page.getByTestId('patient-actions-trigger').first().click();
    await expect(page).toHaveURL(/\/patients$/);

    // View opens the detail as a slide-over — the URL stays on /patients (no
    // patientId exposed in the path) and the detail renders inside the sheet.
    await page.getByTestId('patient-action-view').first().click();
    await expect(page.getByTestId('patient-detail-sheet')).toBeVisible({ timeout: 5_000 });
    await expect(page.getByTestId('patient-status-badge')).toBeVisible({ timeout: 15_000 });
    await expect(page).toHaveURL(/\/patients$/);
  });

  test('ITAdmin: anonymize workflow', async ({ page }) => {
    await loginAsITAdmin(page);

    const uniqueName = `AnonymizePatient-${Date.now()}`;

    // Register a patient first and wait for the detail page to fully load
    await page.goto('/patients/new');
    await page.getByTestId('patient-full-name-input').fill(uniqueName);
    await page.getByTestId('patient-dob-input').fill('1985-03-20');

    await Promise.all([
      page.waitForResponse(r =>
        r.url().includes('/api/v1/patients') &&
        r.request().method() === 'POST' &&
        r.status() === 201),
      page.getByTestId('patient-form-submit-btn').click(),
    ]);

    await expect(page).toHaveURL(
      /\/patients\/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/,
      { timeout: 10_000 },
    );
    // Confirm detail is loaded before navigating away — ensures patient row is committed
    await expect(page.getByTestId('patient-status-badge')).toBeVisible({ timeout: 15_000 });

    // Navigate to search and open the patient detail as a slide-over
    await page.goto('/patients');
    await page.getByTestId('patient-search-input').fill(uniqueName);
    // Generous timeout: debounce 300ms + API round-trip + Angular render
    await expect(page.getByTestId('patient-row').first()).toBeVisible({ timeout: 10_000 });
    await page.getByTestId('patient-row').first().click();

    // Detail opens in the sheet; the URL stays on /patients (no patientId in path).
    await expect(page.getByTestId('patient-detail-sheet')).toBeVisible({ timeout: 5_000 });
    await expect(page).toHaveURL(/\/patients$/);
    await expect(page.getByTestId('patient-status-badge')).toBeVisible({ timeout: 15_000 });

    // Anonymize opens a confirm dialog: click button then confirm
    await page.getByTestId('patient-anonymize-btn').click();
    await expect(page.getByTestId('anonymize-confirm-btn')).toBeVisible({ timeout: 5_000 });

    await Promise.all([
      page.waitForResponse(r =>
        r.url().includes('/anonymize') &&
        r.request().method() === 'POST' &&
        r.status() === 204),
      page.getByTestId('anonymize-confirm-btn').click(),
    ]);

    // confirmAnonymize() reloads details; wait for the badge to reflect new status
    await expect(page.getByTestId('patient-status-badge')).toHaveText('Anonymized', { timeout: 15_000 });
    await expect(page.getByTestId('patient-edit-btn')).not.toBeVisible({ timeout: 5_000 });
    await expect(page.getByTestId('patient-anonymize-btn')).not.toBeVisible({ timeout: 5_000 });
  });

  test('register form shows an inline required error when full name is blurred empty', async ({ page }) => {
    await loginAsReceptionist(page);
    await page.goto('/patients/new');

    const fullName = page.getByTestId('patient-full-name-input');
    await fullName.click();
    await fullName.blur();

    await expect(page.getByText(/full name is required/i)).toBeVisible({ timeout: 5_000 });
  });

  test('Receptionist: registers a patient with a date of birth picked from the calendar', async ({
    page,
  }) => {
    await loginAsReceptionist(page);

    const uniqueName = `CalendarPatient-${Date.now()}`;

    await page.goto('/patients/new');
    await page.getByTestId('patient-full-name-input').fill(uniqueName);

    // Seed the month, then pick a different day from the popover so the assertion
    // proves the calendar wrote the value rather than the typed text.
    await page.getByTestId('patient-dob-input').fill('1985-03-12');
    await page.getByTestId('patient-dob-trigger').click();
    await expect(page.getByTestId('patient-dob-calendar')).toBeVisible({ timeout: 5_000 });

    await page.getByTestId('patient-dob-day-1985-03-20').click();
    await expect(page.getByTestId('patient-dob-calendar')).toBeHidden({ timeout: 5_000 });
    await expect(page.getByTestId('patient-dob-input')).toHaveValue('1985-03-20');

    await Promise.all([
      page.waitForResponse(r =>
        r.url().includes('/api/v1/patients') &&
        r.request().method() === 'POST' &&
        r.status() === 201),
      page.getByTestId('patient-form-submit-btn').click(),
    ]);

    await expect(page).toHaveURL(
      /\/patients\/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/,
      { timeout: 10_000 },
    );
    await expect(page.getByTestId('patient-status-badge')).toBeVisible({ timeout: 15_000 });
  });

  test('LabTechnician is redirected to /unauthorized when accessing /patients', async ({ page }) => {
    await loginAsLabTechnician(page);
    await page.goto('/patients');
    await expect(page).toHaveURL('/unauthorized', { timeout: 5_000 });
  });

  test('Physician is redirected to /unauthorized when accessing /patients', async ({ page }) => {
    await loginAsPhysician(page);
    await page.goto('/patients');
    await expect(page).toHaveURL('/unauthorized', { timeout: 5_000 });
  });
});

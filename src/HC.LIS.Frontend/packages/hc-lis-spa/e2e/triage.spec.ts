import { test, expect } from '@playwright/test';

// Dev seed users — all share the same password
const RECEPTIONIST_EMAIL = 'receptionist@hclis.local';
const LAB_TECH_EMAIL = 'labtech@hclis.local';
const ITADMIN_EMAIL = 'itadmin@hclis.local';
const PASSWORD = 'Admin1234!';

async function loginAsReceptionist(page: import('@playwright/test').Page) {
  await page.goto('/login');
  await page.getByLabel('Email').fill(RECEPTIONIST_EMAIL);
  await page.getByLabel('Password').fill(PASSWORD);
  await page.getByRole('button', { name: /sign in/i }).click();
  await expect(page).toHaveURL('/orders/new', { timeout: 10_000 });
}

async function loginAsLabTechnician(page: import('@playwright/test').Page) {
  await page.goto('/login');
  await page.getByLabel('Email').fill(LAB_TECH_EMAIL);
  await page.getByLabel('Password').fill(PASSWORD);
  await page.getByRole('button', { name: /sign in/i }).click();
  await expect(page).toHaveURL('/waiting-room', { timeout: 10_000 });
}

async function loginAsITAdmin(page: import('@playwright/test').Page) {
  await page.goto('/login');
  await page.getByLabel('Email').fill(ITADMIN_EMAIL);
  await page.getByLabel('Password').fill(PASSWORD);
  await page.getByRole('button', { name: /sign in/i }).click();
  await expect(page).toHaveURL('/admin/users', { timeout: 10_000 });
}

test.describe('Triage — Role Guard', () => {
  test('Receptionist is redirected to /unauthorized when accessing /triage', async ({ page }) => {
    await loginAsReceptionist(page);
    await page.goto('/triage');
    await expect(page).toHaveURL('/unauthorized', { timeout: 5_000 });
  });
});

test.describe('Triage — Nav Link Visibility', () => {
  test('LabTechnician sees nav-triage-link in shell', async ({ page }) => {
    await loginAsLabTechnician(page);
    await expect(page.getByTestId('nav-triage-link')).toBeVisible({ timeout: 5_000 });
  });

  test('ITAdmin sees nav-triage-link in shell', async ({ page }) => {
    await loginAsITAdmin(page);
    await expect(page.getByTestId('nav-triage-link')).toBeVisible({ timeout: 5_000 });
  });

  test('Receptionist does not see nav-triage-link in shell', async ({ page }) => {
    await loginAsReceptionist(page);
    await expect(page.getByTestId('nav-triage-link')).not.toBeVisible({ timeout: 5_000 });
  });
});

test.describe('Triage — Page Structure', () => {
  test('LabTechnician navigates to /triage and sees the page title', async ({ page }) => {
    await loginAsLabTechnician(page);
    await page.getByTestId('nav-triage-link').click();
    await expect(page).toHaveURL('/triage', { timeout: 10_000 });
    await expect(page.getByTestId('triage-title')).toBeVisible({ timeout: 5_000 });
    await expect(page.getByTestId('triage-title')).toContainText('Triage');
  });

  test('Triage page shows arriving and preparing sections', async ({ page }) => {
    await loginAsLabTechnician(page);
    await page.goto('/triage');
    await expect(page.getByTestId('arriving-section')).toBeVisible({ timeout: 5_000 });
    await expect(page.getByTestId('preparing-section')).toBeVisible({ timeout: 5_000 });
  });
});

test.describe('Triage — Full Workflow', () => {
  // This test requires a CollectionRequest in "Arrived" status to exist in the database.
  // Pre-conditions:
  //   1. A TestOrder with at least one accepted exam must have been placed via the Receptionist flow.
  //   2. The SampleCollection module must have processed the integration event and created a
  //      CollectionRequest, which must subsequently have arrived at the lab (status = Arrived).
  //   3. RabbitMQ must be active so the Outbox relay delivers the integration events.
  // Without this seed data the "arriving-section" will have no patient cards and the workflow
  // cannot be exercised. Use test.fixme until a reliable seed/setup mechanism is in place.
  test.fixme('LabTechnician sends arrived patient to waiting room then creates barcode', async ({ page }) => {
    await loginAsLabTechnician(page);
    await page.goto('/triage');

    // Arriving section must have at least one patient card
    await expect(page.getByTestId('arrived-patient-card').first()).toBeVisible({ timeout: 10_000 });

    // Send the first arrived patient to the waiting room
    await page.getByTestId('arrived-patient-card').first().getByTestId('send-to-waiting-btn').click();
    await page.waitForResponse(
      resp => resp.url().includes('move-to-waiting') && resp.status() === 204
    );

    // Patient card disappears from the arriving section after being sent
    await expect(page.getByTestId('arrived-patient-card')).toHaveCount(0, { timeout: 10_000 });

    // Preparing section must now have the patient card
    await expect(page.getByTestId('preparing-patient-card').first()).toBeVisible({ timeout: 10_000 });

    // Fill in barcode and submit
    await page.getByTestId('preparing-patient-card').first().getByTestId('barcode-value-input').fill('BC-TRIAGE-001');
    await page.getByTestId('preparing-patient-card').first().getByTestId('create-barcode-btn').click();
    await page.waitForResponse(
      resp => resp.url().includes('barcode') && resp.status() === 204
    );
  });
});

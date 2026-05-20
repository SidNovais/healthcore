import { test, expect } from '@playwright/test';

const LAB_TECH_EMAIL = 'labtech@hclis.local';
const RECEPTIONIST_EMAIL = 'receptionist@hclis.local';
const ITADMIN_EMAIL = 'itadmin@hclis.local';
const PASSWORD = 'Admin1234!';

async function loginAsLabTechnician(page: import('@playwright/test').Page) {
  await page.goto('/login');
  await page.getByLabel('Email').fill(LAB_TECH_EMAIL);
  await page.getByLabel('Password').fill(PASSWORD);
  await page.getByRole('button', { name: /sign in/i }).click();
  await expect(page).toHaveURL('/triage', { timeout: 10_000 });
}

async function loginAsReceptionist(page: import('@playwright/test').Page) {
  await page.goto('/login');
  await page.getByLabel('Email').fill(RECEPTIONIST_EMAIL);
  await page.getByLabel('Password').fill(PASSWORD);
  await page.getByRole('button', { name: /sign in/i }).click();
  await expect(page).toHaveURL('/orders/new', { timeout: 10_000 });
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

test.describe('Triage — Navigation', () => {
  test('LabTechnician sees nav-triage-link and no nav-waiting-room-link', async ({ page }) => {
    await loginAsLabTechnician(page);
    await expect(page.getByTestId('nav-triage-link')).toBeVisible({ timeout: 5_000 });
    await expect(page.getByTestId('nav-waiting-room-link')).not.toBeVisible({ timeout: 3_000 });
  });

  test('ITAdmin sees nav-triage-link and no nav-waiting-room-link', async ({ page }) => {
    await loginAsITAdmin(page);
    await page.goto('/triage');
    await expect(page.getByTestId('nav-triage-link')).toBeVisible({ timeout: 5_000 });
    await expect(page.getByTestId('nav-waiting-room-link')).not.toBeVisible({ timeout: 3_000 });
  });

  test('/waiting-room redirects to /triage', async ({ page }) => {
    await loginAsLabTechnician(page);
    await page.goto('/waiting-room');
    await expect(page).toHaveURL('/triage', { timeout: 5_000 });
  });
});

test.describe('Triage — Page Structure', () => {
  test('page title is visible', async ({ page }) => {
    await loginAsLabTechnician(page);
    await expect(page.getByTestId('triage-title')).toContainText('Triage', { timeout: 5_000 });
  });

  test('filter tabs are visible', async ({ page }) => {
    await loginAsLabTechnician(page);
    await expect(page.getByTestId('filter-tab-all')).toBeVisible({ timeout: 5_000 });
    await expect(page.getByTestId('filter-tab-arrived')).toBeVisible({ timeout: 5_000 });
    await expect(page.getByTestId('filter-tab-waiting')).toBeVisible({ timeout: 5_000 });
    await expect(page.getByTestId('filter-tab-called')).toBeVisible({ timeout: 5_000 });
  });
});

test.describe('Triage — Full Workflow', () => {
  test.fixme('full lifecycle: Arrived → Waiting (print modal) → Called → Collected', async ({ page }) => {
    await loginAsLabTechnician(page);
    await page.goto('/triage');

    await expect(page.getByTestId('arrived-group').getByTestId('patient-row').first()).toBeVisible({ timeout: 10_000 });

    await page.getByTestId('arrived-group').getByTestId('patient-row').first()
      .getByTestId('patient-row-menu-btn').click();
    await expect(page.getByTestId('action-send-to-waiting')).toBeVisible({ timeout: 3_000 });

    await page.getByTestId('action-send-to-waiting').click();
    await page.waitForResponse(
      resp => resp.url().includes('move-to-waiting') && resp.status() === 204
    );

    await expect(page.getByTestId('arrived-group').getByTestId('patient-row')).toHaveCount(0, { timeout: 10_000 });
    await expect(page.getByTestId('waiting-group').getByTestId('patient-row').first()).toBeVisible({ timeout: 10_000 });

    await page.getByTestId('waiting-group').getByTestId('patient-row').first()
      .getByTestId('patient-row-menu-btn').click();
    await expect(page.getByTestId('action-print-label')).toBeVisible({ timeout: 3_000 });
    await page.getByTestId('action-print-label').click();
    await expect(page.getByTestId('print-labels-modal')).toBeVisible({ timeout: 5_000 });
    await page.getByTestId('print-modal-cancel-btn').click();
    await expect(page.getByTestId('print-labels-modal')).not.toBeVisible({ timeout: 3_000 });

    await page.getByTestId('waiting-group').getByTestId('patient-row').first()
      .getByTestId('patient-row-menu-btn').click();
    await page.getByTestId('action-call-patient').click();
    await page.waitForResponse(
      resp => resp.url().includes('call-patient') && resp.status() === 204
    );

    await expect(page.getByTestId('called-group').getByTestId('patient-row').first()).toBeVisible({ timeout: 10_000 });

    // Click "Record Collection" — should load sample cards from the backend
    await page.getByTestId('called-group').getByTestId('patient-row').first()
      .getByTestId('patient-row-menu-btn').click();
    await page.getByTestId('action-record-collection').click();

    await page.waitForResponse(
      resp => resp.url().includes('/samples') && resp.status() === 200
    );

    await expect(page.getByTestId('sample-card').first()).toBeVisible({ timeout: 5_000 });

    // Collect each sample card
    const sampleCards = page.getByTestId('sample-card');
    const cardCount = await sampleCards.count();
    for (let i = 0; i < cardCount; i++) {
      await sampleCards.nth(i).getByTestId('sample-collect-btn').click();
      await page.waitForResponse(resp => resp.url().includes('collect') && resp.status() === 204);
    }

    await expect(page.getByTestId('called-group').getByTestId('patient-row')).toHaveCount(0, { timeout: 10_000 });
  });
});

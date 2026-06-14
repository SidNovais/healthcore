import { test, expect } from '@playwright/test';

// Dev seed user — Receptionist role required for /orders/new
const RECEPTIONIST_EMAIL = 'receptionist@hclis.local';
const RECEPTIONIST_PASSWORD = 'Admin1234!';

async function loginAsReceptionist(page: import('@playwright/test').Page) {
  await page.goto('/login');
  await page.getByLabel('Email').fill(RECEPTIONIST_EMAIL);
  await page.getByLabel('Password').fill(RECEPTIONIST_PASSWORD);
  await page.getByRole('button', { name: /sign in/i }).click();
  await expect(page).toHaveURL('/orders/new', { timeout: 10_000 });
}

// Mock the patient search endpoint and select a patient via the picker.
// The fake patient ID is the well-known seed UUID accepted by the TestOrders module.
async function pickPatient(
  page: import('@playwright/test').Page,
  patientId = '00000000-0000-0000-0000-000000000001',
): Promise<void> {
  await page.route(/\/api\/v1\/patients(\?.*)?$/, async route => {
    if (route.request().method() === 'GET') {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([{
          id: patientId,
          fullName: 'Seeded Test Patient',
          dateOfBirth: '1990-01-01',
          documentId: 'SEED-001',
          status: 'Active',
        }]),
      });
    } else {
      await route.continue();
    }
  });

  await page.getByTestId('patient-picker-input').fill('Seeded');
  await page.waitForResponse(r =>
    r.url().includes('/api/v1/patients') &&
    r.request().method() === 'GET',
  );
  await expect(page.getByTestId('patient-picker-result-item').first()).toBeVisible({ timeout: 5_000 });
  await page.getByTestId('patient-picker-result-item').first().click();
  await expect(page.getByTestId('patient-picker-selected-card')).toBeVisible({ timeout: 5_000 });
}

test.describe('Test Order Request', () => {
  test.beforeEach(async ({ context }) => {
    await context.clearCookies();
  });

  test('Receptionist creates an order and requests an exam — confirmation visible', async ({ page }) => {
    await loginAsReceptionist(page);
    await pickPatient(page);

    await Promise.all([
      page.waitForResponse(r =>
        r.url().includes('/api/v1/orders') &&
        r.request().method() === 'POST' &&
        r.status() === 201),
      page.getByTestId('create-order-submit-btn').click(),
    ]);

    await expect(page.getByTestId('exam-section')).toBeVisible({ timeout: 5_000 });

    // Fill exam mnemonic + container type and request exam
    await page.getByTestId('exam-mnemonic-input').fill('GLU');
    await page.getByTestId('container-type-input').fill('RedTop');
    await page.getByTestId('request-exam-btn').click();

    // Confirmation appears
    await expect(page.getByTestId('exam-added-confirmation')).toBeVisible({ timeout: 5_000 });
  });

  test('Receptionist cannot access /waiting-room (role guard)', async ({ page }) => {
    await loginAsReceptionist(page);
    await page.goto('/waiting-room');
    await expect(page).toHaveURL('/unauthorized', { timeout: 5_000 });
  });
});

const LAB_TECH_EMAIL = 'labtech@hclis.local';
const LAB_TECH_PASSWORD = 'Admin1234!';

async function loginAsLabTechnician(page: import('@playwright/test').Page) {
  await page.goto('/login');
  await page.getByLabel('Email').fill(LAB_TECH_EMAIL);
  await page.getByLabel('Password').fill(LAB_TECH_PASSWORD);
  await page.getByRole('button', { name: /sign in/i }).click();
  await expect(page).toHaveURL('/triage', { timeout: 10_000 });
}

test.describe('Order List', () => {
  test.beforeEach(async ({ context }) => {
    await context.clearCookies();
  });

  test('Receptionist sees order list table at /orders', async ({ page }) => {
    await loginAsReceptionist(page);
    await page.goto('/orders');
    await expect(page.getByTestId('order-list-table')).toBeVisible({ timeout: 5_000 });
  });

  test('Clicking an order row navigates to /orders/:id', async ({ page }) => {
    await loginAsReceptionist(page);

    // Create an order so the list has at least one row
    await pickPatient(page);
    await page.getByTestId('create-order-submit-btn').click();
    await expect(page.getByTestId('exam-section')).toBeVisible({ timeout: 5_000 });

    await page.goto('/orders');
    await expect(page.getByTestId('order-list-table')).toBeVisible({ timeout: 5_000 });

    await page.getByTestId('order-list-row').first().click();
    await expect(page).toHaveURL(
      /\/orders\/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/,
      { timeout: 5_000 }
    );
  });

  test('Order list displays patient name instead of GUID', async ({ page }) => {
    await loginAsReceptionist(page);

    await page.route(/\/api\/v1\/orders(\?.*)?$/, async route => {
      if (route.request().method() === 'GET') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify([{
            orderId: '00000000-0000-0000-0000-000000000099',
            patientId: '00000000-0000-0000-0000-000000000001',
            patientName: 'Maria Silva',
            requestedBy: '00000000-0000-0000-0000-000000000002',
            orderPriority: 'Routine',
            requestedAt: new Date().toISOString(),
            itemCount: 1,
          }]),
        });
      } else {
        await route.continue();
      }
    });

    await page.goto('/orders');
    await expect(page.getByTestId('order-list-table')).toBeVisible({ timeout: 5_000 });

    const patientCell = page.getByTestId('patient-name-cell').first();
    await expect(patientCell).toBeVisible({ timeout: 5_000 });
    await expect(patientCell).toHaveText('Maria Silva');
    await expect(patientCell).not.toContainText(/^[0-9a-f]{8}-/i);
  });

  test('LabTechnician is redirected to /unauthorized when accessing /orders', async ({ page }) => {
    await loginAsLabTechnician(page);
    await page.goto('/orders');
    await expect(page).toHaveURL('/unauthorized', { timeout: 5_000 });
  });
});

const PHYSICIAN_EMAIL = 'physician@hclis.local';
const PHYSICIAN_PASSWORD = 'Admin1234!';

async function loginAsPhysician(page: import('@playwright/test').Page) {
  await page.goto('/login');
  await page.getByLabel('Email').fill(PHYSICIAN_EMAIL);
  await page.getByLabel('Password').fill(PHYSICIAN_PASSWORD);
  await page.getByRole('button', { name: /sign in/i }).click();
  await expect(page).toHaveURL('/worklist', { timeout: 10_000 });
}

test.describe('Order Detail', () => {
  test.beforeEach(async ({ context }) => {
    await context.clearCookies();
  });

  test('Receptionist sees order detail page at /orders/:id', async ({ page }) => {
    await loginAsReceptionist(page);
    await pickPatient(page);

    await page.getByTestId('create-order-submit-btn').click();
    await expect(page.getByTestId('exam-section')).toBeVisible({ timeout: 5_000 });
    await page.getByTestId('exam-mnemonic-input').fill('GLU');
    await page.getByTestId('container-type-input').fill('RedTop');
    await page.getByTestId('request-exam-btn').click();
    await expect(page.getByTestId('exam-added-confirmation')).toBeVisible({ timeout: 5_000 });

    await page.goto('/orders');
    await expect(page.getByTestId('order-list-table')).toBeVisible({ timeout: 5_000 });
    await page.getByTestId('order-list-row').first().click();
    await expect(page).toHaveURL(
      /\/orders\/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/,
      { timeout: 5_000 }
    );

    await expect(page.getByTestId('order-detail')).toBeVisible({ timeout: 5_000 });
  });

  test('Order detail page shows exam items table with one row', async ({ page }) => {
    await loginAsReceptionist(page);
    await pickPatient(page);

    await page.getByTestId('create-order-submit-btn').click();
    await expect(page.getByTestId('exam-section')).toBeVisible({ timeout: 5_000 });
    const orderId = (await page.locator('.order-id').textContent())!.trim();
    await page.getByTestId('exam-mnemonic-input').fill('GLU');
    await page.getByTestId('container-type-input').fill('RedTop');
    await page.getByTestId('request-exam-btn').click();
    await expect(page.getByTestId('exam-added-confirmation')).toBeVisible({ timeout: 5_000 });

    // Navigate via /orders to allow the exam projection time to commit (Quartz outbox ~2 s)
    await page.goto('/orders');
    await expect(page.getByTestId('order-list-table')).toBeVisible({ timeout: 5_000 });
    await page.goto(`/orders/${orderId}`);

    await expect(page.getByTestId('exam-items-table')).toBeVisible({ timeout: 5_000 });
    await expect(page.getByTestId('exam-item-row').first()).toBeVisible({ timeout: 5_000 });
  });

  test('LabTechnician is redirected to /unauthorized when accessing /orders/:id', async ({ page }) => {
    await loginAsLabTechnician(page);
    await page.goto('/orders/00000000-0000-0000-0000-000000000001');
    await expect(page).toHaveURL('/unauthorized', { timeout: 5_000 });
  });

  test('Receptionist can Accept a Requested exam item — status updates to Accepted', async ({ page }) => {
    await loginAsReceptionist(page);
    await pickPatient(page);

    await page.getByTestId('create-order-submit-btn').click();
    await expect(page.getByTestId('exam-section')).toBeVisible({ timeout: 5_000 });
    const orderId = (await page.locator('.order-id').textContent())!.trim();
    await page.getByTestId('exam-mnemonic-input').fill('GLU');
    await page.getByTestId('container-type-input').fill('RedTop');
    await page.getByTestId('request-exam-btn').click();
    await expect(page.getByTestId('exam-added-confirmation')).toBeVisible({ timeout: 5_000 });

    await page.goto('/orders');
    await expect(page.getByTestId('order-list-table')).toBeVisible({ timeout: 5_000 });
    await page.goto(`/orders/${orderId}`);

    await expect(page.getByTestId('exam-items-table')).toBeVisible({ timeout: 5_000 });
    await expect(page.getByTestId('accept-btn').first()).toBeVisible({ timeout: 5_000 });

    await page.getByTestId('accept-btn').first().click();

    await expect(page.getByTestId('item-status').first()).toHaveText('Accepted', { timeout: 10_000 });
  });

  test('Receptionist can Reject an exam item with a reason — status updates to Rejected', async ({ page }) => {
    await loginAsReceptionist(page);
    await pickPatient(page);

    await page.getByTestId('create-order-submit-btn').click();
    await expect(page.getByTestId('exam-section')).toBeVisible({ timeout: 5_000 });
    const orderId = (await page.locator('.order-id').textContent())!.trim();
    await page.getByTestId('exam-mnemonic-input').fill('HGB');
    await page.getByTestId('container-type-input').fill('EDTA');
    await page.getByTestId('request-exam-btn').click();
    await expect(page.getByTestId('exam-added-confirmation')).toBeVisible({ timeout: 5_000 });

    await page.goto('/orders');
    await expect(page.getByTestId('order-list-table')).toBeVisible({ timeout: 5_000 });
    await page.goto(`/orders/${orderId}`);

    await expect(page.getByTestId('exam-items-table')).toBeVisible({ timeout: 5_000 });
    await page.getByTestId('reject-btn').first().click();

    await expect(page.getByTestId('reject-reason-form')).toBeVisible({ timeout: 5_000 });
    await page.getByTestId('reject-reason-input').fill('Hemolyzed sample');
    await page.getByTestId('confirm-reject-btn').click();

    await expect(page.getByTestId('item-status').first()).toHaveText('Rejected', { timeout: 10_000 });
  });

  test('Receptionist can Cancel a Requested exam item — status updates to Canceled', async ({ page }) => {
    await loginAsReceptionist(page);
    await pickPatient(page);

    await page.getByTestId('create-order-submit-btn').click();
    await expect(page.getByTestId('exam-section')).toBeVisible({ timeout: 5_000 });
    const orderId = (await page.locator('.order-id').textContent())!.trim();
    await page.getByTestId('exam-mnemonic-input').fill('TSH');
    await page.getByTestId('container-type-input').fill('GoldTop');
    await page.getByTestId('request-exam-btn').click();
    await expect(page.getByTestId('exam-added-confirmation')).toBeVisible({ timeout: 5_000 });

    await page.goto('/orders');
    await expect(page.getByTestId('order-list-table')).toBeVisible({ timeout: 5_000 });
    await page.goto(`/orders/${orderId}`);
    await expect(page.getByTestId('exam-items-table')).toBeVisible({ timeout: 5_000 });

    await expect(page.getByTestId('cancel-btn').first()).toBeVisible({ timeout: 5_000 });
    await page.getByTestId('cancel-btn').first().click();
    await expect(page.getByTestId('item-status').first()).toHaveText('Canceled', { timeout: 10_000 });
  });

  test('Receptionist can Place a Requested exam On Hold with reason — status updates to OnHold', async ({ page }) => {
    await loginAsReceptionist(page);
    await pickPatient(page);

    await page.getByTestId('create-order-submit-btn').click();
    await expect(page.getByTestId('exam-section')).toBeVisible({ timeout: 5_000 });
    const orderId = (await page.locator('.order-id').textContent())!.trim();
    await page.getByTestId('exam-mnemonic-input').fill('CRP');
    await page.getByTestId('container-type-input').fill('GoldTop');
    await page.getByTestId('request-exam-btn').click();
    await expect(page.getByTestId('exam-added-confirmation')).toBeVisible({ timeout: 5_000 });

    await page.goto('/orders');
    await expect(page.getByTestId('order-list-table')).toBeVisible({ timeout: 5_000 });
    await page.goto(`/orders/${orderId}`);
    await expect(page.getByTestId('exam-items-table')).toBeVisible({ timeout: 5_000 });

    await expect(page.getByTestId('on-hold-btn').first()).toBeVisible({ timeout: 5_000 });
    await page.getByTestId('on-hold-btn').first().click();

    await expect(page.getByTestId('on-hold-reason-form')).toBeVisible({ timeout: 5_000 });
    await page.getByTestId('on-hold-reason-input').fill('Awaiting physician confirmation');
    await page.getByTestId('confirm-on-hold-btn').click();

    await expect(page.getByTestId('item-status').first()).toHaveText('OnHold', { timeout: 10_000 });
  });

  test('Shows exam-action-error when an exam action returns a 409 business rule error', async ({ page }) => {
    await loginAsReceptionist(page);
    await pickPatient(page);

    await page.getByTestId('create-order-submit-btn').click();
    await expect(page.getByTestId('exam-section')).toBeVisible({ timeout: 5_000 });
    const orderId = (await page.locator('.order-id').textContent())!.trim();
    await page.getByTestId('exam-mnemonic-input').fill('LDL');
    await page.getByTestId('container-type-input').fill('GoldTop');
    await page.getByTestId('request-exam-btn').click();
    await expect(page.getByTestId('exam-added-confirmation')).toBeVisible({ timeout: 5_000 });

    await page.goto('/orders');
    await expect(page.getByTestId('order-list-table')).toBeVisible({ timeout: 5_000 });
    await page.goto(`/orders/${orderId}`);
    await expect(page.getByTestId('exam-items-table')).toBeVisible({ timeout: 5_000 });

    await page.route(`**/orders/${orderId}/exams/**/accept`, route =>
      route.fulfill({
        status: 409,
        contentType: 'application/problem+json',
        body: JSON.stringify({ detail: 'Cannot accept item: already accepted.' }),
      })
    );

    await expect(page.getByTestId('accept-btn').first()).toBeVisible({ timeout: 5_000 });
    await page.getByTestId('accept-btn').first().click();

    await expect(page.getByTestId('exam-action-error')).toBeVisible({ timeout: 5_000 });
    await expect(page.getByTestId('exam-action-error')).toContainText('Cannot accept item: already accepted.');
  });
});

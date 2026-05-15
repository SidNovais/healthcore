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

test.describe('Test Order Request', () => {
  test('Receptionist creates an order and requests an exam — confirmation visible', async ({ page }) => {
    await loginAsReceptionist(page);

    // Fill patient ID and create order
    await page.getByTestId('patient-id-input').fill('00000000-0000-0000-0000-000000000001');
    await page.getByTestId('create-order-btn').click();

    // Exam section appears after order creation
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
  await expect(page).toHaveURL('/waiting-room', { timeout: 10_000 });
}

test.describe('Order List', () => {
  test('Receptionist sees order list table at /orders', async ({ page }) => {
    await loginAsReceptionist(page);
    await page.goto('/orders');
    await expect(page.getByTestId('order-list-table')).toBeVisible({ timeout: 5_000 });
  });

  test('Clicking an order row navigates to /orders/:id', async ({ page }) => {
    await loginAsReceptionist(page);

    // Create an order so the list has at least one row
    await page.getByTestId('patient-id-input').fill('00000000-0000-0000-0000-000000000001');
    await page.getByTestId('create-order-btn').click();
    await expect(page.getByTestId('exam-section')).toBeVisible({ timeout: 5_000 });

    await page.goto('/orders');
    await expect(page.getByTestId('order-list-table')).toBeVisible({ timeout: 5_000 });

    await page.getByTestId('order-list-row').first().click();
    await expect(page).toHaveURL(
      /\/orders\/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/,
      { timeout: 5_000 }
    );
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
  test('Receptionist sees order detail page at /orders/:id', async ({ page }) => {
    await loginAsReceptionist(page);

    await page.getByTestId('patient-id-input').fill('00000000-0000-0000-0000-000000000001');
    await page.getByTestId('create-order-btn').click();
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

    await page.getByTestId('patient-id-input').fill('00000000-0000-0000-0000-000000000001');
    await page.getByTestId('create-order-btn').click();
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

    await page.getByTestId('patient-id-input').fill('00000000-0000-0000-0000-000000000001');
    await page.getByTestId('create-order-btn').click();
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

    await page.getByTestId('patient-id-input').fill('00000000-0000-0000-0000-000000000001');
    await page.getByTestId('create-order-btn').click();
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
});

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

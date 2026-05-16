import { test, expect } from '@playwright/test';

// Dev seed user from UserAccessModule_SeedRootUser migration
const ROOT_EMAIL = 'root@hclis.local';
const ROOT_PASSWORD = 'Admin1234!';

test.describe('Authentication', () => {
  test.beforeEach(async ({ context }) => {
    await context.clearCookies();
  });
  test('ITAdmin login redirects to /admin/users', async ({ page }) => {
    await page.goto('/login');

    await page.getByLabel('Email').fill(ROOT_EMAIL);
    await page.getByLabel('Password').fill(ROOT_PASSWORD);
    await page.getByRole('button', { name: /sign in/i }).click();

    await expect(page).toHaveURL('/admin/users', { timeout: 10_000 });
  });

  test('invalid credentials shows error message and stays on /login', async ({ page }) => {
    await page.goto('/login');

    await page.getByLabel('Email').fill('nobody@hclis.local');
    await page.getByLabel('Password').fill('wrong-password-123');
    await page.getByRole('button', { name: /sign in/i }).click();

    await expect(page.getByRole('alert')).toBeVisible({ timeout: 5_000 });
    await expect(page).toHaveURL('/login');
  });

  test('unauthenticated access to protected route redirects to /login', async ({ page }) => {
    await page.goto('/admin/users');

    await expect(page).toHaveURL('/login', { timeout: 5_000 });
  });
});

const RECEPTIONIST_EMAIL = 'receptionist@hclis.local';
const RECEPTIONIST_PASSWORD = 'Admin1234!';
const PHYSICIAN_EMAIL = 'physician@hclis.local';
const PHYSICIAN_PASSWORD = 'Admin1234!';
const LAB_TECH_EMAIL = 'labtech@hclis.local';
const LAB_TECH_PASSWORD = 'Admin1234!';

async function loginAsReceptionist(page: import('@playwright/test').Page) {
  await page.goto('/login');
  await page.getByLabel('Email').fill(RECEPTIONIST_EMAIL);
  await page.getByLabel('Password').fill(RECEPTIONIST_PASSWORD);
  await page.getByRole('button', { name: /sign in/i }).click();
  await expect(page).toHaveURL('/orders/new', { timeout: 10_000 });
}

async function loginAsPhysician(page: import('@playwright/test').Page) {
  await page.goto('/login');
  await page.getByLabel('Email').fill(PHYSICIAN_EMAIL);
  await page.getByLabel('Password').fill(PHYSICIAN_PASSWORD);
  await page.getByRole('button', { name: /sign in/i }).click();
  await expect(page).toHaveURL('/worklist', { timeout: 10_000 });
}

async function loginAsLabTechnician(page: import('@playwright/test').Page) {
  await page.goto('/login');
  await page.getByLabel('Email').fill(LAB_TECH_EMAIL);
  await page.getByLabel('Password').fill(LAB_TECH_PASSWORD);
  await page.getByRole('button', { name: /sign in/i }).click();
  await expect(page).toHaveURL('/waiting-room', { timeout: 10_000 });
}

test.describe('Shell Navigation', () => {
  test.beforeEach(async ({ context }) => {
    await context.clearCookies();
  });

  test('Receptionist sees Orders nav link', async ({ page }) => {
    await loginAsReceptionist(page);
    await expect(page.getByTestId('nav-orders-link')).toBeVisible({ timeout: 5_000 });
  });

  test('Physician sees Orders nav link', async ({ page }) => {
    await loginAsPhysician(page);
    await expect(page.getByTestId('nav-orders-link')).toBeVisible({ timeout: 5_000 });
  });

  test('ITAdmin sees Orders nav link', async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel('Email').fill(ROOT_EMAIL);
    await page.getByLabel('Password').fill(ROOT_PASSWORD);
    await page.getByRole('button', { name: /sign in/i }).click();
    await expect(page).toHaveURL('/admin/users', { timeout: 10_000 });
    await expect(page.getByTestId('nav-orders-link')).toBeVisible({ timeout: 5_000 });
  });

  test('LabTechnician does not see Orders nav link', async ({ page }) => {
    await loginAsLabTechnician(page);
    await expect(page.getByTestId('nav-orders-link')).not.toBeVisible({ timeout: 5_000 });
  });
});

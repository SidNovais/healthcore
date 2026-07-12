import { test, expect } from '@playwright/test';
import {
  ITADMIN_EMAIL,
  PASSWORD,
  loginAsITAdmin,
  loginAsLabTechnician,
  loginAsPhysician,
  loginAsReceptionist,
} from './fixtures/auth';

test.describe('Authentication', () => {
  test.beforeEach(async ({ context }) => {
    await context.clearCookies();
  });
  test('ITAdmin login redirects to /admin/users', async ({ page }) => {
    await page.goto('/login');

    await page.getByLabel('Email').fill(ITADMIN_EMAIL);
    await page.getByLabel('Password').fill(PASSWORD);
    await page.getByRole('button', { name: /sign in/i }).click();

    await expect(page).toHaveURL('/admin/users', { timeout: 10_000 });
  });

  test('invalid credentials shows error message and stays on /login', async ({ page }) => {
    await page.goto('/login');

    await page.getByLabel('Email').fill('nobody@hclis.local');
    await page.getByLabel('Password').fill('wrong-password-123');
    await Promise.all([
      page.waitForResponse(r =>
        r.url().includes('/api/v1/auth/login') && r.status() === 400),
      page.getByRole('button', { name: /sign in/i }).click(),
    ]);

    await expect(page.getByRole('alert')).toBeVisible({ timeout: 5_000 });
    await expect(page).toHaveURL('/login');
  });

  test('unauthenticated access to protected route redirects to /login', async ({ page }) => {
    await page.goto('/admin/users');

    await expect(page).toHaveURL('/login', { timeout: 10_000 });
  });
});

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
    await loginAsITAdmin(page);
    await expect(page.getByTestId('nav-orders-link')).toBeVisible({ timeout: 5_000 });
  });

  test('LabTechnician does not see Orders nav link', async ({ page }) => {
    await loginAsLabTechnician(page);
    await expect(page.getByTestId('nav-orders-link')).not.toBeVisible({ timeout: 5_000 });
  });
});

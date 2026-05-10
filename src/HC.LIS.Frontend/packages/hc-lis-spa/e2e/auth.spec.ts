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

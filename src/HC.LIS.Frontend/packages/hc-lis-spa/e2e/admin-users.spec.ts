// e2e/admin-users.spec.ts
import { test, expect } from '@playwright/test';

const ITADMIN_EMAIL = 'itadmin@hclis.local';
const ITADMIN_PASSWORD = 'Admin1234!';

async function loginAsITAdmin(page: import('@playwright/test').Page) {
  await page.goto('/login');
  await page.getByLabel('Email').fill(ITADMIN_EMAIL);
  await page.getByLabel('Password').fill(ITADMIN_PASSWORD);
  await page.getByRole('button', { name: /sign in/i }).click();
  await expect(page).toHaveURL('/admin/users', { timeout: 10_000 });
}

test.describe('User Management', () => {
  test('ITAdmin sees user list on login', async ({ page }) => {
    await loginAsITAdmin(page);
    await expect(page.getByTestId('users-title')).toBeVisible({ timeout: 5_000 });
  });

  test('ITAdmin cannot access /worklist (role guard)', async ({ page }) => {
    await loginAsITAdmin(page);
    await page.goto('/worklist');
    await expect(page).toHaveURL('/unauthorized', { timeout: 5_000 });
  });

  test('full create-user workflow: open form → fill fields → submit → new user appears in list', async ({ page }) => {
    await loginAsITAdmin(page);

    // Open create user form
    await page.getByTestId('create-user-btn').click();
    await expect(page.getByTestId('create-user-form')).toBeVisible({ timeout: 3_000 });

    // Fill in user details
    await page.getByTestId('user-email-input').fill('newuser@hclis.local');
    await page.getByTestId('user-fullname-input').fill('New User');
    await page.getByTestId('user-birthdate-input').fill('1995-06-15');
    await page.getByTestId('user-gender-select').selectOption('Male');
    await page.getByTestId('user-role-select').selectOption('LabTechnician');

    // Submit
    await page.getByTestId('submit-create-user-btn').click();
    await page.waitForResponse(resp => resp.url().includes('/api/v1/users') && resp.status() === 201);

    // New user appears in the list
    await expect(page.getByTestId('user-row').filter({ hasText: 'newuser@hclis.local' })).toBeVisible({ timeout: 5_000 });
  });
});

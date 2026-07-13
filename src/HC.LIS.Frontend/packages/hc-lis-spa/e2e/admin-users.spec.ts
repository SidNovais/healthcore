// e2e/admin-users.spec.ts
import { test, expect } from '@playwright/test';
import { loginAsITAdmin } from './fixtures/auth';

test.describe('User Management', () => {
  test('ITAdmin sees user list on login', async ({ page }) => {
    await loginAsITAdmin(page);
    await expect(page.getByTestId('users-title')).toBeVisible({ timeout: 5_000 });
  });

  test('ITAdmin can access /orders/new', async ({ page }) => {
    await loginAsITAdmin(page);
    await page.goto('/orders/new');
    await expect(page).toHaveURL('/orders/new', { timeout: 5_000 });
  });

  test('ITAdmin can access /triage via /waiting-room redirect', async ({ page }) => {
    await loginAsITAdmin(page);
    await page.goto('/waiting-room');
    await expect(page).toHaveURL('/triage', { timeout: 5_000 });
  });

  test('ITAdmin can access /worklist', async ({ page }) => {
    await loginAsITAdmin(page);
    await page.goto('/worklist');
    await expect(page).toHaveURL('/worklist', { timeout: 5_000 });
  });

  test('full create-user workflow: open form → fill fields → submit → new user appears in list', async ({ page }) => {
    await loginAsITAdmin(page);

    const uniqueEmail = `newuser-${Date.now()}@hclis.local`;

    // Open create user form
    await page.getByTestId('create-user-btn').click();
    await expect(page.getByTestId('create-user-form')).toBeVisible({ timeout: 3_000 });

    // Fill in user details
    await page.getByTestId('user-email-input').fill(uniqueEmail);
    await page.getByTestId('user-fullname-input').fill('New User');
    await page.getByTestId('user-birthdate-input').fill('1995-06-15');
    await page.getByTestId('user-gender-select').selectOption('Male');
    await page.getByTestId('user-role-select').selectOption('LabTechnician');

    // Submit — register listener before click to avoid race condition
    await Promise.all([
      page.waitForResponse(resp => resp.url().includes('/api/v1/users') && resp.status() === 201),
      page.getByTestId('submit-create-user-btn').click(),
    ]);

    // New user appears in the list
    await expect(page.getByTestId('user-row').filter({ hasText: uniqueEmail })).toBeVisible({ timeout: 5_000 });
  });

  test('change-role workflow: menu → confirm dialog → success toast', async ({ page }) => {
    await loginAsITAdmin(page);

    // Create a throwaway user so we never mutate a seed account's role.
    const uniqueEmail = `roletest-${Date.now()}@hclis.local`;
    await page.getByTestId('create-user-btn').click();
    await expect(page.getByTestId('create-user-form')).toBeVisible({ timeout: 3_000 });
    await page.getByTestId('user-email-input').fill(uniqueEmail);
    await page.getByTestId('user-fullname-input').fill('Role Test');
    await page.getByTestId('user-birthdate-input').fill('1990-02-02');
    await page.getByTestId('user-gender-select').selectOption('Female');
    await page.getByTestId('user-role-select').selectOption('LabTechnician');
    await Promise.all([
      page.waitForResponse(resp => resp.url().includes('/api/v1/users') && resp.status() === 201),
      page.getByTestId('submit-create-user-btn').click(),
    ]);

    const row = page.getByTestId('user-row').filter({ hasText: uniqueEmail });
    await expect(row).toBeVisible({ timeout: 5_000 });

    // Open the role menu and pick a new role → confirmation dialog appears.
    await row.getByTestId('user-role-trigger').click();
    await page.getByTestId('user-role-option-Physician').click();
    await expect(page.getByTestId('confirm-role-change-btn')).toBeVisible({ timeout: 3_000 });

    await Promise.all([
      page.waitForResponse(resp => resp.url().includes('/role') && resp.request().method() === 'PUT'),
      page.getByTestId('confirm-role-change-btn').click(),
    ]);

    await expect(page.getByTestId('role-change-toast')).toBeVisible({ timeout: 5_000 });
  });
});

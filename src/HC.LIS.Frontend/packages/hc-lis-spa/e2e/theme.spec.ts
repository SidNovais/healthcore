import { test, expect, type Page } from '@playwright/test';
import {
  loginAsITAdmin,
  loginAsLabTechnician,
  loginAsPhysician,
  loginAsReceptionist,
} from './fixtures/auth';

const htmlTheme = (page: Page) => page.locator('html');

test.describe('Theme Toggle', () => {
  test.beforeEach(async ({ context }) => {
    await context.clearCookies();
  });

  test('toggle switches data-theme on <html> from light to dark and back', async ({ page }) => {
    await loginAsITAdmin(page);

    await expect(htmlTheme(page)).toHaveAttribute('data-theme', 'light');

    await page.getByTestId('theme-toggle-btn').click();
    await expect(htmlTheme(page)).toHaveAttribute('data-theme', 'dark');

    await page.getByTestId('theme-toggle-btn').click();
    await expect(htmlTheme(page)).toHaveAttribute('data-theme', 'light');
  });

  test('chosen theme persists across a full page reload', async ({ page }) => {
    await loginAsITAdmin(page);

    await page.getByTestId('theme-toggle-btn').click();
    await expect(htmlTheme(page)).toHaveAttribute('data-theme', 'dark');

    await page.reload();

    await expect(htmlTheme(page)).toHaveAttribute('data-theme', 'dark', { timeout: 10_000 });
    const stored = await page.evaluate(() => localStorage.getItem('hc-lis-theme'));
    expect(stored).toBe('dark');
  });

  test('toggle button label swaps with the active theme', async ({ page }) => {
    await loginAsITAdmin(page);

    const toggle = page.getByTestId('theme-toggle-btn');
    await expect(toggle).toHaveAttribute('aria-label', 'Switch to dark mode');

    await toggle.click();
    await expect(toggle).toHaveAttribute('aria-label', 'Switch to light mode');
  });

  for (const [role, login] of [
    ['Receptionist', loginAsReceptionist],
    ['LabTechnician', loginAsLabTechnician],
    ['Physician', loginAsPhysician],
  ] as const) {
    test(`${role} can toggle the theme`, async ({ page }) => {
      await login(page);

      await page.getByTestId('theme-toggle-btn').click();
      await expect(htmlTheme(page)).toHaveAttribute('data-theme', 'dark');
    });
  }
});

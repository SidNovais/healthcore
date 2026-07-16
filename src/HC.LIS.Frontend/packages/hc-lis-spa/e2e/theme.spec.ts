import { test, expect, type Page } from '@playwright/test';
import {
  loginAsITAdmin,
  loginAsLabTechnician,
  loginAsPhysician,
  loginAsReceptionist,
} from './fixtures/auth';

const htmlTheme = (page: Page) => page.locator('html');

// The toggle lives inside the shell user menu (Phase 13) — it must be opened first.
// Activating a menu item closes the menu, so each toggle needs its own open.
async function toggleTheme(page: Page): Promise<void> {
  await page.getByTestId('user-menu-trigger').click();
  await page.getByTestId('theme-toggle-btn').click();
}

test.describe('Theme Toggle', () => {
  test.beforeEach(async ({ context }) => {
    await context.clearCookies();
  });

  test('toggle switches data-theme on <html> from light to dark and back', async ({ page }) => {
    await loginAsITAdmin(page);

    await expect(htmlTheme(page)).toHaveAttribute('data-theme', 'light');

    await toggleTheme(page);
    await expect(htmlTheme(page)).toHaveAttribute('data-theme', 'dark');

    await toggleTheme(page);
    await expect(htmlTheme(page)).toHaveAttribute('data-theme', 'light');
  });

  test('chosen theme persists across a full page reload', async ({ page }) => {
    await loginAsITAdmin(page);

    await toggleTheme(page);
    await expect(htmlTheme(page)).toHaveAttribute('data-theme', 'dark');

    await page.reload();

    await expect(htmlTheme(page)).toHaveAttribute('data-theme', 'dark', { timeout: 10_000 });
    const stored = await page.evaluate(() => localStorage.getItem('hc-lis-theme'));
    expect(stored).toBe('dark');
  });

  test('toggle item label swaps with the active theme', async ({ page }) => {
    await loginAsITAdmin(page);

    await page.getByTestId('user-menu-trigger').click();
    await expect(page.getByTestId('theme-toggle-btn')).toHaveAttribute(
      'aria-label',
      'Switch to dark mode',
    );

    await page.getByTestId('theme-toggle-btn').click();
    await expect(htmlTheme(page)).toHaveAttribute('data-theme', 'dark');

    await page.getByTestId('user-menu-trigger').click();
    await expect(page.getByTestId('theme-toggle-btn')).toHaveAttribute(
      'aria-label',
      'Switch to light mode',
    );
  });

  for (const [role, login] of [
    ['Receptionist', loginAsReceptionist],
    ['LabTechnician', loginAsLabTechnician],
    ['Physician', loginAsPhysician],
  ] as const) {
    test(`${role} can toggle the theme`, async ({ page }) => {
      await login(page);

      await toggleTheme(page);
      await expect(htmlTheme(page)).toHaveAttribute('data-theme', 'dark');
    });
  }
});

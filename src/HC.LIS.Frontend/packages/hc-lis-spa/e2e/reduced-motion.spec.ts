import { test, expect, type Locator, type Page } from '@playwright/test';
import { loginAsReceptionist } from './fixtures/auth';

// Phase 4 gate: every GSAP animation must be skipped under prefers-reduced-motion, leaving
// content fully visible (never stuck mid-fade at opacity 0 / visibility:hidden). We emulate
// the OS preference and assert the animated targets settle at full opacity immediately.

/** Computed opacity of the element, as the browser reports it (a stringified float). */
async function opacityOf(locator: Locator): Promise<number> {
  return locator.evaluate(el => parseFloat(getComputedStyle(el).opacity));
}

test.describe('Reduced motion', () => {
  test.beforeEach(async ({ context, page }) => {
    await context.clearCookies();
    await page.emulateMedia({ reducedMotion: 'reduce' });
  });

  test('login card is fully visible — entrance fade is skipped', async ({ page }) => {
    await page.goto('/login');

    const card = page.getByTestId('login-card');
    await expect(card).toBeVisible();
    expect(await opacityOf(card)).toBe(1);
  });

  test('route crossfade is skipped — outlet stays fully visible across navigation', async ({
    page,
  }) => {
    await loginAsReceptionist(page);

    const outlet = page.getByTestId('outlet-wrapper');
    await expect(outlet).toBeVisible();
    expect(await opacityOf(outlet)).toBe(1);

    // Trigger a route change; the crossfade would drop the outlet to opacity 0 under motion.
    await page.getByTestId('nav-orders-link').click();
    await expect(page).toHaveURL('/orders', { timeout: 10_000 });
    await expect(outlet).toBeVisible();
    expect(await opacityOf(outlet)).toBe(1);
  });

  test('list-row stagger is skipped — rows render at full opacity', async ({ page }) => {
    await loginAsReceptionist(page);
    await page.getByTestId('nav-orders-link').click();
    await expect(page).toHaveURL('/orders', { timeout: 10_000 });

    const rows = page.getByTestId('order-list-row');
    // The list may legitimately be empty in a fresh environment; only assert when rows exist.
    const count = await rows.count();
    if (count === 0) {
      test.skip(true, 'No seeded orders to exercise the row stagger.');
    }
    for (let i = 0; i < count; i++) {
      expect(await opacityOf(rows.nth(i))).toBe(1);
    }
  });

  test('error page card is fully visible — entrance fade is skipped', async ({ page }) => {
    await loginAsReceptionist(page);
    await page.goto('/unauthorized');

    const card = page.getByTestId('error-card');
    await expect(card).toBeVisible();
    expect(await opacityOf(card)).toBe(1);
  });
});

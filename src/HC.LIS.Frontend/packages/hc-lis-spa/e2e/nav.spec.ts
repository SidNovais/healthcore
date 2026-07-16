import { test, expect } from '@playwright/test';
import { loginAsITAdmin, loginAsReceptionist } from './fixtures/auth';

// Shell navigation — active-state indicator + role badge. Existing nav visibility/role-guard
// assertions live in auth.spec.ts and triage.spec.ts; this spec covers the Phase 2 active
// indicator (exposed accessibly via aria-current="page") and the role badge.
test.describe('Shell navigation', () => {
  test('active nav item reflects the current route via aria-current', async ({ page }) => {
    await loginAsITAdmin(page);

    // ITAdmin lands on /admin/users — the Users link is the active one.
    await expect(page.getByTestId('nav-users-link')).toHaveAttribute('aria-current', 'page');
    await expect(page.getByTestId('nav-orders-link')).not.toHaveAttribute('aria-current', 'page');

    // Navigating swaps the active indicator to the new route's link.
    await page.getByTestId('nav-orders-link').click();
    await expect(page).toHaveURL(/\/orders$/, { timeout: 10_000 });
    await expect(page.getByTestId('nav-orders-link')).toHaveAttribute('aria-current', 'page');
    await expect(page.getByTestId('nav-users-link')).not.toHaveAttribute('aria-current', 'page');
  });

  test('role badge shows the signed-in user role inside the user menu', async ({ page }) => {
    await loginAsITAdmin(page);

    // Phase 13 moved the role badge into the avatar-triggered user menu.
    await page.getByTestId('user-menu-trigger').click();

    await expect(page.getByTestId('shell-role-badge')).toHaveText('ITAdmin', { timeout: 5_000 });
  });

  test('user menu trigger shows the signed-in user identity', async ({ page }) => {
    await loginAsITAdmin(page);

    const trigger = page.getByTestId('user-menu-trigger');
    await expect(trigger).toHaveAttribute('aria-expanded', 'false');
    await expect(page.getByTestId('user-menu-avatar')).toHaveText('IT', { timeout: 5_000 });

    await trigger.click();
    await expect(trigger).toHaveAttribute('aria-expanded', 'true');
    await expect(page.getByTestId('user-menu-name')).toHaveText('itadmin@hclis.local');
  });

  // Phase 14 — the order-detail breadcrumb replaces the ad-hoc "← Back to Orders" link.
  test('order-detail breadcrumb trails back to the orders list', async ({ page }) => {
    await loginAsReceptionist(page);
    await page.getByTestId('nav-orders-link').click();
    await expect(page).toHaveURL(/\/orders$/, { timeout: 10_000 });

    const firstRow = page.getByTestId('order-list-row').first();
    if ((await firstRow.count()) === 0) {
      test.skip(true, 'No seeded orders to open a detail page for.');
    }
    await firstRow.click();
    await expect(page).toHaveURL(/\/orders\/[^/]+$/, { timeout: 10_000 });

    await expect(page.getByTestId('order-breadcrumb-page')).toHaveText('Order Detail');
    await page.getByTestId('order-breadcrumb-link-0').click();

    await expect(page).toHaveURL(/\/orders$/, { timeout: 10_000 });
  });
});

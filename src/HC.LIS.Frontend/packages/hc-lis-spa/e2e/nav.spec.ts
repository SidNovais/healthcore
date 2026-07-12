import { test, expect } from '@playwright/test';
import { loginAsITAdmin } from './fixtures/auth';

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

  test('role badge shows the signed-in user role', async ({ page }) => {
    await loginAsITAdmin(page);

    await expect(page.getByTestId('shell-role-badge')).toHaveText('ITAdmin', { timeout: 5_000 });
  });
});

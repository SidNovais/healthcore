// e2e/admin-physicians.spec.ts
import { test, expect, type Page } from '@playwright/test';
import { loginAsITAdmin, loginAsReceptionist } from './fixtures/auth';

/** The registry row for a physician, addressed by the name we registered it under. */
function row(page: Page, fullName: string) {
  return page.getByTestId('physician-list-row').filter({ hasText: fullName });
}

async function submitForm(page: Page, method: 'POST' | 'PUT'): Promise<void> {
  await Promise.all([
    page.waitForResponse(
      r => r.url().includes('/api/v1/physicians') && r.request().method() === method && r.status() < 400,
    ),
    page.getByTestId('physician-form-submit-btn').click(),
  ]);
}

/**
 * Two things make a plain locator assertion unreliable here.
 *
 * `PhysicianDetails` is a projection, so a 2xx from the API does not mean the row is
 * queryable yet — and the list is fetched once per page load, so retrying the locator
 * alone can never succeed; the page has to be reloaded until the projection catches up.
 *
 * The registry is also paginated at 10 rows with no search box, and every run leaves its
 * physicians behind (there is no delete endpoint), so a newly registered name is often
 * not on page one. Walk the pages rather than assuming it is.
 */
async function expectRowEventually(page: Page, fullName: string, status?: string): Promise<void> {
  await expect(async () => {
    await page.goto('/admin/physicians');
    await expect(page.getByTestId('physician-list-table')).toBeVisible({ timeout: 5_000 });

    const pageButtons = page.locator('[data-testid^="physicians-pagination-page-"]');
    const pageCount = Math.max(1, await pageButtons.count());

    for (let p = 1; p <= pageCount && (await row(page, fullName).count()) === 0; p++) {
      if (p === 1) continue;
      // Compare raw innerText in-browser: toHaveText normalises whitespace, which makes
      // "the rows changed" pass instantly and lets the walk skip a page.
      const before = await page.getByTestId('physician-list-row').first().innerText();
      await page.getByTestId(`physicians-pagination-page-${p}`).click();
      await page.waitForFunction(
        prev => document.querySelector<HTMLElement>('[data-testid="physician-list-row"]')?.innerText !== prev,
        before,
        { timeout: 5_000 },
      );
    }

    if ((await row(page, fullName).count()) === 0) {
      throw new Error(`physician "${fullName}" is not on any of the ${pageCount} registry pages yet`);
    }

    const target = row(page, fullName);
    await expect(target).toBeVisible({ timeout: 2_000 });
    if (status) {
      await expect(target.getByTestId('physician-status-badge')).toHaveText(status, {
        timeout: 2_000,
      });
    }
  }).toPass({ timeout: 30_000, intervals: [500, 1_000, 2_000] });
}

test.describe('Physician Registry', () => {
  test.beforeEach(async ({ context }) => {
    await context.clearCookies();
  });

  test('full workflow: register a physician → edit it → deactivate it', async ({ page }) => {
    // Three API round trips behind a login; the 30s default leaves nothing spare.
    test.setTimeout(60_000);
    await loginAsITAdmin(page);

    await page.getByTestId('nav-physicians-link').click();
    await expect(page).toHaveURL('/admin/physicians', { timeout: 10_000 });
    await expect(page.getByTestId('physicians-title')).toBeVisible({ timeout: 5_000 });

    const fullName = `Registry Test ${Date.now()}`;
    const licence = `CRM-${Date.now()}`;

    await page.getByTestId('create-physician-btn').click();
    await expect(page.getByTestId('physician-form')).toBeVisible({ timeout: 3_000 });
    await page.getByTestId('physician-full-name-input').fill(fullName);
    await page.getByTestId('physician-licence-input').fill(licence);
    await submitForm(page, 'POST');

    await expectRowEventually(page, fullName, 'Active');

    // Edit — the form reopens carrying the stored values, so only the name changes.
    const editedName = `${fullName} Jr`;
    await row(page, fullName).getByTestId('physician-actions-trigger').click();
    await page.getByTestId('physician-action-edit').click();
    await expect(page.getByTestId('physician-full-name-input')).toHaveValue(fullName);
    await expect(page.getByTestId('physician-licence-input')).toHaveValue(licence);
    await page.getByTestId('physician-full-name-input').fill(editedName);
    await submitForm(page, 'PUT');

    await expectRowEventually(page, editedName);

    // Deactivate — confirmed in a dialog, then the row's badge flips in place.
    await row(page, editedName).getByTestId('physician-actions-trigger').click();
    await page.getByTestId('physician-action-deactivate').click();
    await expect(page.getByTestId('physician-status-dialog')).toBeVisible({ timeout: 3_000 });
    await Promise.all([
      page.waitForResponse(
        r => r.url().includes('/deactivate') && r.request().method() === 'POST' && r.status() < 400,
      ),
      page.getByTestId('confirm-physician-status-btn').click(),
    ]);

    await expect(page.getByTestId('physician-status-toast')).toBeVisible({ timeout: 5_000 });
    await expectRowEventually(page, editedName, 'Inactive');
  });

  test('a deactivated physician can be reactivated from the same menu', async ({ page }) => {
    test.setTimeout(60_000);
    await loginAsITAdmin(page);

    // Register and deactivate through the API — this test is about the reactivate path,
    // not about re-driving the form a second time.
    const fullName = `Reactivate Test ${Date.now()}`;
    const created = await page.request.post('/api/v1/physicians', {
      data: { fullName, licenceNumber: `CRM-${Date.now()}` },
    });
    expect(created.status()).toBe(201);
    const { id } = (await created.json()) as { id: string };
    expect((await page.request.post(`/api/v1/physicians/${id}/deactivate`)).ok()).toBe(true);

    await expectRowEventually(page, fullName, 'Inactive');

    await row(page, fullName).getByTestId('physician-actions-trigger').click();
    await page.getByTestId('physician-action-reactivate').click();
    await Promise.all([
      page.waitForResponse(
        r => r.url().includes('/reactivate') && r.request().method() === 'POST' && r.status() < 400,
      ),
      page.getByTestId('confirm-physician-status-btn').click(),
    ]);

    await expectRowEventually(page, fullName, 'Active');
  });

  test('Receptionist is redirected to /unauthorized when accessing /admin/physicians', async ({
    page,
  }) => {
    await loginAsReceptionist(page);
    await page.goto('/admin/physicians');
    await expect(page).toHaveURL('/unauthorized', { timeout: 5_000 });
  });

  test('the registry nav link is offered to ITAdmin only', async ({ page }) => {
    await loginAsReceptionist(page);
    await expect(page.getByTestId('nav-physicians-link')).toBeHidden();

    await loginAsITAdmin(page);
    await expect(page.getByTestId('nav-physicians-link')).toBeVisible({ timeout: 5_000 });
  });
});

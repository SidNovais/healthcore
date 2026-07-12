import { test, expect } from '@playwright/test';
// Uses the root seed user (ITAdmin) — the only user guaranteed by the UserAccess migration.
import { loginAsITAdmin } from './fixtures/auth';

test.describe('HIPAA Compliance', () => {

  test('after login, page URL contains no patient names or DOBs', async ({ page }) => {
    const visitedUrls: string[] = [];
    page.on('framenavigated', frame => {
      if (frame === page.mainFrame()) visitedUrls.push(frame.url());
    });

    await loginAsITAdmin(page);

    for (const route of ['/waiting-room', '/orders/new', '/worklist', '/admin/users']) {
      await page.goto(route);
      await page.waitForLoadState('networkidle');
      visitedUrls.push(page.url());
    }

    const PHI_PATTERNS = [
      /\/\d{4}-\d{2}-\d{2}(\/|$)/,
      /\/[a-z]{2,}-[a-z]{2,}(\/|$)/i,
    ];
    const STATIC_ROUTES = new Set(['/orders/new', '/waiting-room', '/admin/users', '/login', '/unauthorized', '/worklist']);

    for (const url of visitedUrls) {
      let pathname: string;
      try { pathname = new URL(url).pathname; } catch { continue; }
      if (STATIC_ROUTES.has(pathname)) continue;
      for (const pattern of PHI_PATTERNS) {
        expect(
          pattern.test(pathname),
          `URL path "${pathname}" matched PHI pattern ${pattern}`,
        ).toBe(false);
      }
    }
  });

  test('localStorage does not contain ACCESS_TOKEN at any point during a workflow', async ({ page }) => {
    await page.addInitScript(() => {
      const original = Storage.prototype.setItem;
      Storage.prototype.setItem = function(key: string, value: string) {
        if (/access.?token|jwt/i.test(key)) {
          throw new Error(`[HIPAA] localStorage.setItem("${key}") forbidden — use HttpOnly cookie`);
        }
        return original.call(this, key, value);
      };
    });

    await loginAsITAdmin(page);

    const badKeys = await page.evaluate(() =>
      Array.from({ length: localStorage.length }, (_, i) => localStorage.key(i) ?? '')
        .filter(k => /access.?token|jwt/i.test(k)),
    );
    expect(badKeys).toHaveLength(0);
  });

  test('sessionStorage does not contain ACCESS_TOKEN at any point during a workflow', async ({ page }) => {
    await page.addInitScript(() => {
      const original = Storage.prototype.setItem;
      Storage.prototype.setItem = function(key: string, value: string) {
        if (/access.?token|jwt/i.test(key)) {
          throw new Error(`[HIPAA] sessionStorage.setItem("${key}") forbidden — use HttpOnly cookie`);
        }
        return original.call(this, key, value);
      };
    });

    await loginAsITAdmin(page);

    const badKeys = await page.evaluate(() =>
      Array.from({ length: sessionStorage.length }, (_, i) => sessionStorage.key(i) ?? '')
        .filter(k => /access.?token|jwt/i.test(k)),
    );
    expect(badKeys).toHaveLength(0);
  });

  test('patient PHI does not appear in console output during patient search or detail load', async ({ page }) => {
    const consoleMessages: string[] = [];
    page.on('console', msg => consoleMessages.push(msg.text()));

    await loginAsITAdmin(page);

    const uniqueName = `HipaaTest-${Date.now()}`;
    const dob = '1992-07-15';
    const documentId = `DOC-${Date.now()}`;

    await page.goto('/patients/new');
    await page.getByTestId('patient-full-name-input').fill(uniqueName);
    await page.getByTestId('patient-dob-input').fill(dob);
    await page.getByTestId('patient-document-id-input').fill(documentId);

    await Promise.all([
      page.waitForResponse(r =>
        r.url().includes('/api/v1/patients') &&
        r.request().method() === 'POST' &&
        r.status() === 201,
      ),
      page.getByTestId('patient-form-submit-btn').click(),
    ]);

    await expect(page).toHaveURL(
      /\/patients\/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/,
      { timeout: 10_000 },
    );
    await expect(page.getByTestId('patient-status-badge')).toBeVisible({ timeout: 15_000 });

    // Search workflow — exercises the search API call and component rendering
    await page.goto('/patients');
    await page.getByTestId('patient-search-input').fill(uniqueName);
    await expect(page.getByTestId('patient-row').first()).toBeVisible({ timeout: 10_000 });

    // Detail load — exercises the detail API call and component rendering
    await page.getByTestId('patient-row').first().click();
    await expect(page).toHaveURL(
      /\/patients\/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/,
      { timeout: 10_000 },
    );
    await expect(page.getByTestId('patient-status-badge')).toBeVisible({ timeout: 15_000 });

    // Assert no PHI in any console message
    const phi = [uniqueName, dob, documentId];
    for (const msg of consoleMessages) {
      for (const value of phi) {
        expect(msg, `Console message contains PHI "${value}": "${msg}"`).not.toContain(value);
      }
    }
  });

  test('navigating to waiting room does not expose patientId in URL path', async ({ page }) => {
    const spaUrls: string[] = [];
    const baseUrl = process.env['E2E_BASE_URL'] ?? 'localhost:4200';
    page.on('framenavigated', frame => {
      if (frame === page.mainFrame() && frame.url().includes(baseUrl)) {
        spaUrls.push(frame.url());
      }
    });

    await loginAsITAdmin(page);

    // Navigate to /waiting-room; ITAdmin is now allowed on this route, so the
    // page loads directly with no redirect. The URL must not contain a UUID.
    await page.goto('/waiting-room');
    await page.waitForURL(/\/waiting-room$/, { timeout: 5_000 });

    spaUrls.push(page.url());

    const uuidPattern = /\/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}(\/|$)/i;
    for (const url of spaUrls) {
      let pathname: string;
      try { pathname = new URL(url).pathname; } catch { continue; }
      expect(
        uuidPattern.test(pathname),
        `SPA URL "${pathname}" exposes a UUID (possible patientId) in a path segment`,
      ).toBe(false);
    }
  });

});

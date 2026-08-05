import { expect, type Page } from '@playwright/test';

export interface SeededPhysician {
  id: string;
  fullName: string;
  licenceNumber: string;
}

// Registry search is prefix-only (`term%`), so a picker query must start at the first
// character of the stored name — never mid-string.
const SEARCH_PREFIX_LENGTH = 12;

function searchPrefix(fullName: string): string {
  return fullName.slice(0, SEARCH_PREFIX_LENGTH);
}

/**
 * Registers a referring physician through the real API and waits until the read model can
 * find it. Deliberately not a `page.route` mock the way `pickPatient` fakes patients: a
 * mocked id has no PhysicianDetails row behind it, so the order it is used on is rejected
 * once order creation enforces a registered physician.
 *
 * The caller must already be logged in as a role holding the OrderEntry policy
 * (Receptionist or ITAdmin) — `page.request` shares the page's cookie jar.
 */
export async function ensurePhysician(
  page: Page,
  fullName = `Physician-${Date.now()}`,
): Promise<SeededPhysician> {
  const licenceNumber = `CRM-${Date.now()}`;

  const response = await page.request.post('/api/v1/physicians', {
    data: { fullName, licenceNumber },
  });
  expect(response.status()).toBe(201);
  const { id } = (await response.json()) as { id: string };

  await expect
    .poll(
      async () => {
        const search = await page.request.get(
          `/api/v1/physicians?search=${encodeURIComponent(searchPrefix(fullName))}`,
        );
        const results = (await search.json()) as { id: string }[];
        return results.some(p => p.id === id);
      },
      { timeout: 15_000 },
    )
    .toBe(true);

  return { id, fullName, licenceNumber };
}

/** Selects an already-registered physician through the new-order typeahead. */
export async function pickPhysician(page: Page, fullName: string): Promise<void> {
  await page.getByTestId('physician-picker-input').fill(searchPrefix(fullName));
  await page.waitForResponse(
    r =>
      r.url().includes('/api/v1/physicians') &&
      r.request().method() === 'GET' &&
      r.status() === 200,
  );

  // The prefix matches every physician an earlier run left behind, so target ours by name
  // rather than assuming it sorts first.
  const ourResult = page.getByTestId('physician-picker-result-item').filter({ hasText: fullName });
  await expect(ourResult).toBeVisible({ timeout: 5_000 });
  await ourResult.click();

  await expect(page.getByTestId('physician-picker-selected-card')).toBeVisible({ timeout: 5_000 });
}

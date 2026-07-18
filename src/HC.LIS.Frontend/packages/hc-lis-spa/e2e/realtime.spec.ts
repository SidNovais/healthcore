import { test, expect, type Page } from '@playwright/test';
import { loginAsReceptionist } from './fixtures/auth';

// The well-known seed patient id the TestOrders module accepts (see orders.spec.ts).
const SEED_PATIENT_ID = '00000000-0000-0000-0000-000000000001';

// Mocks patient search and selects the seed patient via the picker, so an order can be created
// without depending on pre-seeded patient data.
async function pickSeedPatient(page: Page): Promise<void> {
  await page.route(/\/api\/v1\/patients(\?.*)?$/, async route => {
    if (route.request().method() === 'GET') {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([{
          id: SEED_PATIENT_ID,
          fullName: 'Seeded Test Patient',
          dateOfBirth: '1990-01-01',
          documentId: 'SEED-001',
          status: 'Active',
        }]),
      });
    } else {
      await route.continue();
    }
  });

  await page.getByTestId('patient-picker-input').fill('Seeded');
  await page.waitForResponse(r => r.url().includes('/api/v1/patients') && r.request().method() === 'GET');
  await page.getByTestId('patient-picker-result-item').first().click();
  await expect(page.getByTestId('patient-picker-selected-card')).toBeVisible({ timeout: 5_000 });
}

test.describe('Real-time feed — connection', () => {
  test.beforeEach(async ({ context }) => {
    await context.clearCookies();
  });

  test('the live indicator connects once authenticated', async ({ page }) => {
    await loginAsReceptionist(page);
    await expect(page.getByTestId('live-indicator'))
      .toHaveAttribute('data-status', 'live', { timeout: 10_000 });
  });

  test('the live indicator reports a dropped connection', async ({ page }) => {
    // Abort every attempt to open the stream so the client can never connect.
    await page.route('**/api/v1/events/stream', route => route.abort());
    await loginAsReceptionist(page);
    await expect(page.getByTestId('live-indicator'))
      .toHaveAttribute('data-status', 'reconnecting', { timeout: 10_000 });
  });
});

test.describe('Real-time feed — cross-session updates', () => {
  // Seeds its own order, so it does not depend on pre-existing data. Requires the full stack
  // (API + RabbitMQ + database), because the change reaches the second session through the
  // outbox → integration event → SSE pipeline.
  test('a canceled exam appears live in another session without a refresh', async ({ browser }) => {
    const watcherContext = await browser.newContext();
    const actorContext = await browser.newContext();
    const watcher = await watcherContext.newPage();
    const actor = await actorContext.newPage();

    try {
      await loginAsReceptionist(actor);
      await loginAsReceptionist(watcher);

      // Actor creates an order and requests one exam.
      await pickSeedPatient(actor);
      await actor.getByTestId('create-order-submit-btn').click();
      await expect(actor.getByTestId('exam-section')).toBeVisible({ timeout: 5_000 });
      const orderId = (await actor.locator('.order-id').textContent())!.trim();
      await actor.getByTestId('exam-mnemonic-input').fill('GLU');
      await actor.getByTestId('container-type-input').fill('RedTop');
      await actor.getByTestId('request-exam-btn').click();
      await expect(actor.getByTestId('exam-added-confirmation')).toBeVisible({ timeout: 5_000 });

      // Both sessions open the same order detail (via /orders so the projection commits first).
      await actor.goto('/orders');
      await actor.goto(`/orders/${orderId}`);
      await expect(actor.getByTestId('exam-items-table')).toBeVisible({ timeout: 5_000 });

      await watcher.goto('/orders');
      await watcher.goto(`/orders/${orderId}`);
      await expect(watcher.getByTestId('exam-item-row').first()).toBeVisible({ timeout: 10_000 });

      // The watcher's feed must be live before the actor acts.
      await expect(watcher.getByTestId('live-indicator'))
        .toHaveAttribute('data-status', 'live', { timeout: 10_000 });

      // Actor cancels the exam.
      await actor.getByTestId('exam-actions-trigger').first().click();
      await expect(actor.getByTestId('cancel-btn').first()).toBeVisible({ timeout: 5_000 });
      await actor.getByTestId('cancel-btn').first().click();
      await expect(actor.getByTestId('item-status').first()).toHaveText('Canceled', { timeout: 10_000 });

      // The watcher updates live — no navigation or refresh on its side.
      await expect(watcher.getByTestId('item-status').first())
        .toHaveText('Canceled', { timeout: 15_000 });
    } finally {
      await watcherContext.close();
      await actorContext.close();
    }
  });
});

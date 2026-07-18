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

// Opens an order's detail and waits for the exam row, retrying the navigation until the
// outbox-driven exam projection (Quartz, ~2s) has committed.
async function openOrderWithExam(page: Page, orderId: string): Promise<void> {
  for (let attempt = 0; attempt < 8; attempt++) {
    await page.goto(`/orders/${orderId}`);
    const visible = await page.getByTestId('exam-item-row').first()
      .isVisible({ timeout: 4_000 }).catch(() => false);
    if (visible) return;
  }
  await expect(page.getByTestId('exam-item-row').first()).toBeVisible({ timeout: 5_000 });
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
  // outbox → integration event → SSE pipeline. Uses Accept — every exam status transition
  // travels this same path, so one proves the pipeline for all of them.
  test('an exam status change appears live in another session without a refresh', async ({ browser }) => {
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
      await actor.getByTestId('exam-mnemonic-input').fill('ACC');
      await actor.getByTestId('container-type-input').fill('RedTop');
      await actor.getByTestId('request-exam-btn').click();
      await expect(actor.getByTestId('exam-added-confirmation')).toBeVisible({ timeout: 5_000 });

      // Both sessions open the same order detail once the exam projection has committed.
      await openOrderWithExam(actor, orderId);
      await openOrderWithExam(watcher, orderId);

      // The watcher's feed must be live before the actor acts.
      await expect(watcher.getByTestId('live-indicator'))
        .toHaveAttribute('data-status', 'live', { timeout: 10_000 });

      // Actor accepts the exam.
      await actor.getByTestId('exam-actions-trigger').first().click();
      await expect(actor.getByTestId('accept-btn').first()).toBeVisible({ timeout: 5_000 });
      await actor.getByTestId('accept-btn').first().click();
      await expect(actor.getByTestId('item-status').first()).toHaveText('Accepted', { timeout: 10_000 });

      // The watcher updates live — no navigation or refresh on its side.
      await expect(watcher.getByTestId('item-status').first())
        .toHaveText('Accepted', { timeout: 15_000 });
    } finally {
      await watcherContext.close();
      await actorContext.close();
    }
  });

  test('a new order and its item count appear live in the orders list', async ({ browser }) => {
    const watcherContext = await browser.newContext();
    const actorContext = await browser.newContext();
    const watcher = await watcherContext.newPage();
    const actor = await actorContext.newPage();

    try {
      await loginAsReceptionist(actor);
      await loginAsReceptionist(watcher);

      // Watcher observes the orders list with a live feed.
      await watcher.goto('/orders');
      await expect(watcher.getByTestId('order-list-table')).toBeVisible({ timeout: 5_000 });
      await expect(watcher.getByTestId('live-indicator'))
        .toHaveAttribute('data-status', 'live', { timeout: 10_000 });

      // Actor creates an order and requests one exam.
      await pickSeedPatient(actor);
      await actor.getByTestId('create-order-submit-btn').click();
      await expect(actor.getByTestId('exam-section')).toBeVisible({ timeout: 5_000 });
      const orderId = (await actor.locator('.order-id').textContent())!.trim();
      await actor.getByTestId('exam-mnemonic-input').fill('GLU');
      await actor.getByTestId('container-type-input').fill('RedTop');
      await actor.getByTestId('request-exam-btn').click();
      await expect(actor.getByTestId('exam-added-confirmation')).toBeVisible({ timeout: 5_000 });

      // The new order appears in the watcher's list live, with its item count — no refresh.
      const newRow = watcher.locator(`[data-testid="order-list-row"][data-order-id="${orderId}"]`);
      await expect(newRow).toBeVisible({ timeout: 15_000 });
      await expect(newRow.locator('.num-col')).toHaveText('1', { timeout: 15_000 });
    } finally {
      await watcherContext.close();
      await actorContext.close();
    }
  });
});

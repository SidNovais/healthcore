import { test, expect } from '@playwright/test';

const LAB_TECH_EMAIL = 'labtech@hclis.local';
const LAB_TECH_PASSWORD = 'Admin1234!';

async function loginAsLabTech(page: import('@playwright/test').Page) {
  await page.goto('/login');
  await page.getByLabel('Email').fill(LAB_TECH_EMAIL);
  await page.getByLabel('Password').fill(LAB_TECH_PASSWORD);
  await page.getByRole('button', { name: /sign in/i }).click();
  await expect(page).toHaveURL('/waiting-room', { timeout: 10_000 });
}

test.describe('Waiting Room', () => {
  test('LabTechnician sees waiting room queue on login', async ({ page }) => {
    await loginAsLabTech(page);
    await expect(page.getByTestId('waiting-room-title')).toBeVisible({ timeout: 5_000 });
  });

  test('LabTechnician cannot access /orders/new (role guard)', async ({ page }) => {
    await loginAsLabTech(page);
    await page.goto('/orders/new');
    await expect(page).toHaveURL('/unauthorized', { timeout: 5_000 });
  });

  // Requires a CollectionRequest in pending state (patient in queue).
  // Depends on TestOrders→SampleCollection integration event flowing via the Outbox relay.
  // Run with RabbitMQ active and a prior order+exam-request to enable this test.
  test.fixme('full collection workflow: call → barcode → record → patient leaves queue', async ({ page }) => {
    await loginAsLabTech(page);

    // Ensure a patient is in queue (requires seed data)
    await expect(page.getByTestId('patient-card').first()).toBeVisible({ timeout: 10_000 });

    // Call the first patient
    await page.getByTestId('patient-card').first().getByTestId('call-patient-btn').click();
    await page.waitForResponse(resp => resp.url().includes('call-patient') && resp.status() === 204);

    // Open collect sample form
    await page.getByTestId('patient-card').first().getByTestId('collect-sample-btn').click();
    await expect(page.getByTestId('collect-sample-form')).toBeVisible({ timeout: 3_000 });

    // Fill barcode
    await page.getByTestId('tube-type-input').fill('EDTA');
    await page.getByTestId('barcode-value-input').fill('BC-TEST-001');

    // Fill patient demographics
    await page.getByTestId('patient-name-input').fill('Test Patient');
    await page.getByTestId('patient-birthdate-input').fill('1990-01-15');
    await page.getByTestId('patient-gender-select').selectOption('M');

    // Submit
    await page.getByTestId('collect-submit-btn').click();

    // Patient no longer in queue (queue refreshes after collection)
    await expect(page.getByTestId('patient-card')).toHaveCount(0, { timeout: 10_000 });
  });
});

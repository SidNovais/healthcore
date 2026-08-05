import { test, expect } from '@playwright/test';
import { loginAsPhysician, loginAsReceptionist } from './fixtures/auth';
import { pickPatient } from './fixtures/orders';
import { ensurePhysician, pickPhysician } from './fixtures/physicians';

test.describe('Physician Picker in New Order', () => {
  test.beforeEach(async ({ context }) => {
    await context.clearCookies();
  });

  test('Receptionist selects a physician via typeahead and the order carries its id', async ({ page }) => {
    await loginAsReceptionist(page);

    const physician = await ensurePhysician(page, `PickerPhysician-${Date.now()}`);

    await page.goto('/orders/new');

    // Submit needs both a patient and a physician.
    await expect(page.getByTestId('create-order-submit-btn')).toBeDisabled();
    await expect(page.getByTestId('physician-picker-input')).toHaveRole('combobox');

    await pickPatient(page);
    await expect(page.getByTestId('create-order-submit-btn')).toBeDisabled();

    await pickPhysician(page, physician.fullName);
    await expect(page.getByTestId('physician-picker-selected-card')).toContainText(physician.fullName);
    await expect(page.getByTestId('physician-picker-selected-card')).toContainText(physician.licenceNumber);
    await expect(page.getByTestId('create-order-submit-btn')).toBeEnabled();

    // The created order must reference the registered physician, not the signed-in user.
    const [request] = await Promise.all([
      page.waitForRequest(
        r => r.url().includes('/api/v1/orders') && r.method() === 'POST',
      ),
      page.waitForResponse(
        r =>
          r.url().includes('/api/v1/orders') &&
          r.request().method() === 'POST' &&
          r.status() === 201,
      ),
      page.getByTestId('create-order-submit-btn').click(),
    ]);

    expect(JSON.parse(request.postData() ?? '{}')).toMatchObject({ requestedBy: physician.id });
    await expect(page.getByTestId('exam-section')).toBeVisible({ timeout: 5_000 });
  });

  test('Receptionist can clear the physician selection — submit becomes disabled again', async ({ page }) => {
    await loginAsReceptionist(page);

    const physician = await ensurePhysician(page, `ClearPhysician-${Date.now()}`);

    await page.goto('/orders/new');
    await pickPatient(page);
    await pickPhysician(page, physician.fullName);
    await expect(page.getByTestId('create-order-submit-btn')).toBeEnabled();

    await page.getByTestId('physician-picker-clear-btn').click();

    await expect(page.getByTestId('physician-picker-selected-card')).not.toBeVisible();
    await expect(page.getByTestId('create-order-submit-btn')).toBeDisabled();
    await expect(page.getByTestId('physician-picker-input')).toBeVisible();
  });

  test('Receptionist quick-adds an unknown physician and it is selected immediately', async ({ page }) => {
    await loginAsReceptionist(page);
    await page.goto('/orders/new');

    const newName = `QuickAdd-${Date.now()}`;

    // A search with no match offers the quick-add affordance.
    await page.getByTestId('physician-picker-input').fill(newName);
    await page.waitForResponse(
      r =>
        r.url().includes('/api/v1/physicians') &&
        r.request().method() === 'GET' &&
        r.status() === 200,
    );
    await expect(page.getByTestId('physician-picker-quick-add-btn')).toBeVisible({ timeout: 5_000 });

    await page.getByTestId('physician-picker-quick-add-btn').click();
    await expect(page.getByTestId('physician-quick-add-dialog')).toBeVisible({ timeout: 5_000 });

    await page.getByTestId('physician-quick-add-name-input').fill(newName);
    await page.getByTestId('physician-quick-add-licence-input').fill('CRM-QA-001');

    await Promise.all([
      page.waitForResponse(
        r =>
          r.url().includes('/api/v1/physicians') &&
          r.request().method() === 'POST' &&
          r.status() === 201,
      ),
      page.getByTestId('physician-quick-add-submit-btn').click(),
    ]);

    // The registered physician is selected without a second search.
    await expect(page.getByTestId('physician-quick-add-dialog')).not.toBeVisible();
    await expect(page.getByTestId('physician-picker-selected-card')).toBeVisible({ timeout: 5_000 });
    await expect(page.getByTestId('physician-picker-selected-card')).toContainText(newName);

    await pickPatient(page);
    await expect(page.getByTestId('create-order-submit-btn')).toBeEnabled();
  });

  test('Physician is redirected to /unauthorized when accessing /orders/new', async ({ page }) => {
    await loginAsPhysician(page);
    await page.goto('/orders/new');
    await expect(page).toHaveURL('/unauthorized', { timeout: 5_000 });
  });
});

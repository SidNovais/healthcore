import { expect, type Page } from '@playwright/test';
import { ensurePhysician, pickPhysician, type SeededPhysician } from './physicians';

/** The well-known seed patient id the TestOrders module accepts. */
export const SEED_PATIENT_ID = '00000000-0000-0000-0000-000000000001';

/** Mocks the patient search and selects the seed patient, so no patient data need be seeded. */
export async function pickPatient(page: Page, patientId = SEED_PATIENT_ID): Promise<void> {
  await page.route(/\/api\/v1\/patients(\?.*)?$/, async route => {
    if (route.request().method() === 'GET') {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([
          {
            id: patientId,
            fullName: 'Seeded Test Patient',
            dateOfBirth: '1990-01-01',
            documentId: 'SEED-001',
            status: 'Active',
          },
        ]),
      });
    } else {
      await route.continue();
    }
  });

  await page.getByTestId('patient-picker-input').fill('Seeded');
  await page.waitForResponse(
    r => r.url().includes('/api/v1/patients') && r.request().method() === 'GET',
  );
  await expect(page.getByTestId('patient-picker-result-item').first()).toBeVisible({
    timeout: 5_000,
  });
  await page.getByTestId('patient-picker-result-item').first().click();
  await expect(page.getByTestId('patient-picker-selected-card')).toBeVisible({ timeout: 5_000 });
}

/**
 * Fills the new-order form to the point where "Create Order" is enabled: the mocked seed
 * patient plus a genuinely registered physician. Every spec that submits an order goes
 * through here, so the physician requirement has exactly one call site to maintain.
 */
export async function startOrder(page: Page, patientId = SEED_PATIENT_ID): Promise<SeededPhysician> {
  const physician = await ensurePhysician(page);
  await pickPatient(page, patientId);
  await pickPhysician(page, physician.fullName);
  await expect(page.getByTestId('create-order-submit-btn')).toBeEnabled();
  return physician;
}

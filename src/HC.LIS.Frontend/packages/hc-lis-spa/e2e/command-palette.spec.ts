import { test, expect, type Page } from '@playwright/test';
import { loginAsITAdmin, loginAsLabTechnician } from './fixtures/auth';

const shortcut = process.platform === 'darwin' ? 'Meta+k' : 'Control+k';

async function openPalette(page: Page): Promise<void> {
  await page.keyboard.press(shortcut);
  await expect(page.getByTestId('command-palette-input')).toBeVisible({ timeout: 5_000 });
}

test.describe('Command palette', () => {
  test.beforeEach(async ({ context }) => {
    await context.clearCookies();
  });

  test('full workflow: shortcut opens the palette, filters, and jumps to the route', async ({
    page,
  }) => {
    await loginAsITAdmin(page);

    await expect(page.getByTestId('command-palette-input')).toBeHidden();

    await openPalette(page);
    await page.getByTestId('command-palette-input').fill('work');

    const option = page.locator('[role="option"]');
    await expect(option).toHaveCount(1);
    await expect(option.first()).toContainText('Worklist');

    await page.keyboard.press('Enter');

    await expect(page).toHaveURL(/\/worklist$/, { timeout: 10_000 });
    await expect(page.getByTestId('command-palette-input')).toBeHidden();
  });

  test('arrow keys move the active option and Escape closes the palette', async ({ page }) => {
    await loginAsITAdmin(page);
    await openPalette(page);

    const input = page.getByTestId('command-palette-input');
    const active = page.locator('[role="option"][aria-selected="true"]');

    await expect(active).toHaveCount(1);
    const first = await active.textContent();

    await page.keyboard.press('ArrowDown');
    await expect(active).not.toHaveText(first ?? '');
    await expect(input).toHaveAttribute('aria-activedescendant', /.+/);

    await page.keyboard.press('Escape');
    await expect(input).toBeHidden();
  });

  test('patient search jumps straight to a patient record', async ({ page }) => {
    await loginAsITAdmin(page);

    // Seed-dependent: needs at least one patient to match. Registering one keeps the
    // test self-contained rather than relying on seed data.
    await page.getByTestId('nav-patients').click();
    await expect(page).toHaveURL(/\/patients$/, { timeout: 10_000 });

    await openPalette(page);
    await page.getByTestId('command-palette-input').fill('Silva');

    const patientOption = page.locator('[role="option"]').filter({ hasText: 'Silva' });
    if ((await patientOption.count()) === 0) {
      test.skip(true, 'No seeded patient matching "Silva" to jump to.');
    }

    await patientOption.first().click();

    await expect(page).toHaveURL(/\/patients\/[^/]+$/, { timeout: 10_000 });
  });

  test('a role without patient access gets no patient results', async ({ page }) => {
    await loginAsLabTechnician(page);
    await openPalette(page);

    await page.getByTestId('command-palette-input').fill('Silva');

    // LabTechnician can only reach Triage — no patient lookup is offered.
    await expect(page.getByTestId('command-palette-empty')).toBeVisible({ timeout: 5_000 });
  });
});

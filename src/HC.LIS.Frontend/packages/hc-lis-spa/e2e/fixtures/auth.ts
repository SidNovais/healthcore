import { expect, type Page } from '@playwright/test';

// Seed users. Root (ITAdmin) comes from the UserAccessModule_SeedRootUser migration and is
// the only account guaranteed in every environment; the role users come from
// UserAccessModule_SeedDevRoleUsers (dev only).
export const PASSWORD = 'Admin1234!';
export const ITADMIN_EMAIL = 'root@hclis.local';
export const RECEPTIONIST_EMAIL = 'receptionist@hclis.local';
export const LAB_TECH_EMAIL = 'labtech@hclis.local';
export const PHYSICIAN_EMAIL = 'physician@hclis.local';

async function login(page: Page, email: string, landingUrl: string): Promise<void> {
  await page.goto('/login');
  await page.getByLabel('Email').fill(email);
  await page.getByLabel('Password').fill(PASSWORD);
  await page.getByRole('button', { name: /sign in/i }).click();
  await expect(page).toHaveURL(landingUrl, { timeout: 10_000 });
}

export async function loginAsITAdmin(page: Page): Promise<void> {
  await login(page, ITADMIN_EMAIL, '/admin/users');
}

export async function loginAsReceptionist(page: Page): Promise<void> {
  await login(page, RECEPTIONIST_EMAIL, '/orders/new');
}

export async function loginAsLabTechnician(page: Page): Promise<void> {
  await login(page, LAB_TECH_EMAIL, '/triage');
}

export async function loginAsPhysician(page: Page): Promise<void> {
  await login(page, PHYSICIAN_EMAIL, '/worklist');
}

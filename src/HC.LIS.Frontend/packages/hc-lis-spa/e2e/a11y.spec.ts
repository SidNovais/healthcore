import { test, expect, type Page } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import {
  loginAsITAdmin,
  loginAsLabTechnician,
  loginAsPhysician,
  loginAsReceptionist,
} from './fixtures/auth';

type Login = (page: Page) => Promise<void>;

// Every static route, paired with a role allowed to view it. Detail routes (/:id) need
// seeded data and are covered by their feature specs instead.
const ROUTES: ReadonlyArray<{ route: string; login: Login | null }> = [
  { route: '/login', login: null },
  { route: '/admin/users', login: loginAsITAdmin },
  { route: '/patients', login: loginAsReceptionist },
  { route: '/patients/new', login: loginAsReceptionist },
  { route: '/orders', login: loginAsReceptionist },
  { route: '/orders/new', login: loginAsReceptionist },
  { route: '/triage', login: loginAsLabTechnician },
  { route: '/worklist', login: loginAsPhysician },
  { route: '/unauthorized', login: loginAsReceptionist },
  // Wildcard route renders NotFoundComponent without requiring auth.
  { route: '/this-route-does-not-exist', login: null },
];

async function scan(page: Page): Promise<void> {
  const results = await new AxeBuilder({ page })
    .withTags(['wcag2a', 'wcag2aa'])
    .analyze();

  expect(
    results.violations,
    results.violations
      .map(v => `${v.id}: ${v.help}\n${v.nodes.map(n => `  ${n.target.join(' ')}`).join('\n')}`)
      .join('\n\n'),
  ).toEqual([]);
}

for (const theme of ['light', 'dark'] as const) {
  test.describe(`Accessibility — ${theme} theme`, () => {
    test.beforeEach(async ({ context, page }) => {
      await context.clearCookies();
      // ThemeService reads this key on startup, so every navigation renders in `theme`.
      await page.addInitScript(t => localStorage.setItem('hc-lis-theme', t), theme);
    });

    for (const { route, login } of ROUTES) {
      test(`${route} has no WCAG A/AA violations`, async ({ page }) => {
        if (login) {
          await login(page);
        }
        await page.goto(route);
        await page.waitForLoadState('networkidle');

        await scan(page);
      });
    }
  });
}

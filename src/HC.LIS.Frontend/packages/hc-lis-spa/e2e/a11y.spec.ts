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

// Both themes are tuned independently (§3), and layout/contrast must hold on a small phone —
// so every route is scanned in light + dark at desktop and 375px (Phase 4 audit gate).
const VIEWPORTS: ReadonlyArray<{ name: string; width: number; height: number }> = [
  { name: 'desktop', width: 1280, height: 800 },
  { name: 'mobile-375', width: 375, height: 812 },
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

for (const viewport of VIEWPORTS) {
  for (const theme of ['light', 'dark'] as const) {
    test.describe(`Accessibility — ${theme} theme @ ${viewport.name}`, () => {
      test.beforeEach(async ({ context, page }) => {
        await context.clearCookies();
        await page.setViewportSize({ width: viewport.width, height: viewport.height });
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
}

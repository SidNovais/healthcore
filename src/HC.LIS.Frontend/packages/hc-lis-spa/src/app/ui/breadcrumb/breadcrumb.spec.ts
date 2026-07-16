import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { HcBreadcrumb, type HcBreadcrumbItem } from './breadcrumb';

@Component({
  imports: [HcBreadcrumb],
  template: `<hc-breadcrumb [items]="items()" testId="order-breadcrumb" />`,
})
class HostComponent {
  readonly items = signal<HcBreadcrumbItem[]>([
    { label: 'Orders', route: '/orders' },
    { label: 'Order Detail' },
  ]);
}

async function render() {
  await TestBed.configureTestingModule({
    imports: [HostComponent],
    providers: [provideRouter([])],
  }).compileComponents();

  const fixture = TestBed.createComponent(HostComponent);
  fixture.detectChanges();
  const host = fixture.nativeElement as HTMLElement;
  return {
    fixture,
    component: fixture.componentInstance,
    nav: () => host.querySelector<HTMLElement>('[data-testid="order-breadcrumb"]')!,
    items: () => Array.from(host.querySelectorAll('ol > li:not([aria-hidden])')),
    separators: () => Array.from(host.querySelectorAll('[aria-hidden="true"]')),
    links: () => Array.from(host.querySelectorAll<HTMLAnchorElement>('a')),
    page: () => host.querySelector<HTMLElement>('[data-testid="order-breadcrumb-page"]'),
  };
}

describe('HcBreadcrumb', () => {
  afterEach(() => TestBed.resetTestingModule());

  it('renders the trail as an ordered list inside a navigation landmark', async () => {
    const { nav, items } = await render();

    expect(nav().getAttribute('role')).toBe('navigation');
    expect(nav().getAttribute('aria-label')).toBe('Breadcrumb');
    expect(nav().querySelector('ol')).not.toBeNull();
    expect(items()).toHaveLength(2);
  });

  it('links every item except the last', async () => {
    const { links } = await render();

    expect(links()).toHaveLength(1);
    expect(links()[0].textContent?.trim()).toBe('Orders');
    expect(links()[0].getAttribute('href')).toBe('/orders');
  });

  it('marks the last item as the current page rather than a link', async () => {
    const { page } = await render();

    expect(page()?.textContent?.trim()).toBe('Order Detail');
    expect(page()?.getAttribute('aria-current')).toBe('page');
    expect(page()?.tagName).not.toBe('A');
  });

  it('hides the separators from assistive tech', async () => {
    const { separators } = await render();

    // One separator between the two crumbs — decorative only.
    expect(separators()).toHaveLength(1);
  });

  it('never links the final crumb even when it carries a route', async () => {
    const { fixture, component, links, page } = await render();

    component.items.set([
      { label: 'Patients', route: '/patients' },
      { label: 'Ada Lovelace', route: '/patients/p-1' },
    ]);
    fixture.detectChanges();

    expect(links()).toHaveLength(1);
    expect(page()?.textContent?.trim()).toBe('Ada Lovelace');
  });

  it('prefixes child testids with the testId input', async () => {
    const { nav } = await render();

    expect(nav().querySelector('[data-testid="order-breadcrumb-link-0"]')).not.toBeNull();
    expect(nav().querySelector('[data-testid="order-breadcrumb-page"]')).not.toBeNull();
  });
});

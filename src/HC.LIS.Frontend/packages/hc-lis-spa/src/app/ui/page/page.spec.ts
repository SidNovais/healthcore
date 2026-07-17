import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { HcPage } from './page';
import type { HcBreadcrumbItem } from '../breadcrumb/breadcrumb';

@Component({
  imports: [HcPage],
  template: `
    <hc-page
      [title]="title()"
      [subtitle]="subtitle()"
      [breadcrumbs]="crumbs()"
      [width]="width()"
      testId="orders"
    >
      <button hc-page-actions data-testid="refresh-btn">Refresh</button>
      <p data-testid="body">Page body</p>
    </hc-page>
  `,
})
class HostComponent {
  readonly title = signal('Orders');
  readonly subtitle = signal<string | undefined>(undefined);
  readonly crumbs = signal<HcBreadcrumbItem[] | undefined>(undefined);
  readonly width = signal<'wide' | 'narrow' | 'full'>('wide');
}

function render() {
  TestBed.configureTestingModule({ providers: [provideRouter([])] });
  const fixture = TestBed.createComponent(HostComponent);
  fixture.detectChanges();
  const host = fixture.nativeElement as HTMLElement;
  return {
    fixture,
    host,
    component: fixture.componentInstance,
    container: () => host.querySelector<HTMLElement>('.hc-page')!,
    heading: () => host.querySelector<HTMLElement>('h1'),
    subtitle: () => host.querySelector<HTMLElement>('.hc-page__subtitle'),
    actions: () => host.querySelector<HTMLElement>('.hc-page__actions'),
    breadcrumb: () => host.querySelector<HTMLElement>('hc-breadcrumb'),
    detect: () => fixture.detectChanges(),
  };
}

describe('HcPage', () => {
  afterEach(() => TestBed.resetTestingModule());

  // The whole point of taking the title as an input rather than a slot: the level is
  // not the page's choice. Nine pages used h1 and three used h2 before this existed.
  it('always renders the title as an h1', () => {
    const { heading } = render();

    expect(heading()).not.toBeNull();
    expect(heading()!.textContent).toContain('Orders');
  });

  it('omits the subtitle entirely when none is given', () => {
    const { subtitle } = render();

    expect(subtitle()).toBeNull();
  });

  it('renders the subtitle when given', () => {
    const { component, detect, subtitle } = render();

    component.subtitle.set('All lab orders');
    detect();

    expect(subtitle()!.textContent).toContain('All lab orders');
  });

  it('projects action content into the header', () => {
    const { actions } = render();

    expect(actions()).not.toBeNull();
    expect(actions()!.querySelector('[data-testid="refresh-btn"]')).not.toBeNull();
  });

  it('projects the default content as the page body', () => {
    const { host } = render();

    expect(host.querySelector('[data-testid="body"]')!.textContent).toContain('Page body');
  });

  // The slot wrapper is ours (authored in page.html), so a scoped selector reaches it —
  // unlike the projected buttons inside it. Guards against the rule silently not
  // matching; asserts a non-token property jsdom can actually resolve.
  it('lays the actions slot out as a row', () => {
    const { actions } = render();

    expect(getComputedStyle(actions()!).display).toBe('flex');
  });

  it('omits the breadcrumb when no trail is given', () => {
    const { breadcrumb } = render();

    expect(breadcrumb()).toBeNull();
  });

  it('renders a breadcrumb trail when given', () => {
    const { component, detect, breadcrumb } = render();

    component.crumbs.set([{ label: 'Orders', route: '/orders' }, { label: 'ORD-1' }]);
    detect();

    expect(breadcrumb()).not.toBeNull();
  });

  // Pages declare what they are; the primitive owns how that centres.
  it('applies the requested measure and always centres', () => {
    const { component, detect, container } = render();

    expect(container().classList).toContain('hc-page--wide');

    component.width.set('narrow');
    detect();
    expect(container().classList).toContain('hc-page--narrow');
    expect(container().classList).not.toContain('hc-page--wide');

    component.width.set('full');
    detect();
    expect(container().classList).toContain('hc-page--full');
  });

  it('defaults to the wide measure', () => {
    const { container } = render();

    expect(container().classList).toContain('hc-page--wide');
  });

  it('exposes the testId on the page container', () => {
    const { container } = render();

    expect(container().getAttribute('data-testid')).toBe('orders');
  });

  // Pages that already had a title testid (worklist-title, users-title) must keep it
  // on the heading when they move onto hc-page — those ids are the e2e regression net
  // and are never renamed. Derived as {testId}-title, as the breadcrumb slot is.
  it('derives the heading testId from the page testId', () => {
    const { heading } = render();

    expect(heading()!.getAttribute('data-testid')).toBe('orders-title');
  });

  // Derivation is the default, but some pages carry ids that predate this primitive and
  // do not follow the pattern: register-patient's heading is register-patient-heading,
  // and order-detail's breadcrumb is order-breadcrumb while its container is
  // order-detail. Those ids are the e2e net, so the page overrides rather than renames.
  it('lets a page override the derived heading testId', () => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({ providers: [provideRouter([])] });

    @Component({
      imports: [HcPage],
      template: `
        <hc-page title="Register New Patient" testId="register-patient" titleTestId="register-patient-heading">
          <p>body</p>
        </hc-page>
      `,
    })
    class Overridden {}

    const fixture = TestBed.createComponent(Overridden);
    fixture.detectChanges();
    const h1 = (fixture.nativeElement as HTMLElement).querySelector('h1')!;

    expect(h1.getAttribute('data-testid')).toBe('register-patient-heading');
  });

  it('lets a page override the derived breadcrumb testId', () => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({ providers: [provideRouter([])] });

    @Component({
      imports: [HcPage],
      template: `
        <hc-page
          title="Order Detail"
          testId="order-detail"
          breadcrumbTestId="order-breadcrumb"
          [breadcrumbs]="[{ label: 'Orders', route: '/orders' }, { label: 'ORD-1' }]"
        >
          <p>body</p>
        </hc-page>
      `,
    })
    class Overridden {}

    const fixture = TestBed.createComponent(Overridden);
    fixture.detectChanges();
    const crumb = (fixture.nativeElement as HTMLElement).querySelector('hc-breadcrumb')!;

    expect(crumb.getAttribute('data-testid')).toBe('order-breadcrumb');
  });

  it('leaves the heading untagged when the page has no testId', () => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({ providers: [provideRouter([])] });

    @Component({
      imports: [HcPage],
      template: `<hc-page title="Orders"><p>body</p></hc-page>`,
    })
    class Bare {}

    const fixture = TestBed.createComponent(Bare);
    fixture.detectChanges();
    const h1 = (fixture.nativeElement as HTMLElement).querySelector('h1')!;

    expect(h1.hasAttribute('data-testid')).toBe(false);
  });
});

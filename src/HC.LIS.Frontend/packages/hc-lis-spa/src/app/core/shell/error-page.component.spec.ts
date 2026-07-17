import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ErrorPageComponent } from './error-page.component';

@Component({
  imports: [ErrorPageComponent],
  template: `
    <app-error-page
      code="403"
      heading="Access Denied"
      message="You don't have permission to view this page."
      icon="lock"
      tone="error"
      fill="main"
      testId="error-card"
    />
  `,
})
class UnauthorizedHost {}

@Component({
  imports: [ErrorPageComponent],
  template: `
    <app-error-page
      code="404"
      heading="Page Not Found"
      message="The page you're looking for doesn't exist."
      fill="viewport"
    />
  `,
})
class NotFoundHost {}

function render<T>(host: new () => T) {
  TestBed.configureTestingModule({ providers: [provideRouter([])] });
  const fixture = TestBed.createComponent(host);
  fixture.detectChanges();
  const el = fixture.nativeElement as HTMLElement;
  return {
    el,
    page: () => el.querySelector<HTMLElement>('.centered-page')!,
    card: () => el.querySelector<HTMLElement>('.error-card')!,
    code: () => el.querySelector<HTMLElement>('.error-code')!,
    icon: () => el.querySelector<HTMLElement>('.error-icon'),
  };
}

describe('ErrorPageComponent', () => {
  afterEach(() => TestBed.resetTestingModule());

  it('renders the code, heading and message', () => {
    const { el, code } = render(UnauthorizedHost);

    expect(code().textContent).toContain('403');
    expect(el.textContent).toContain('Access Denied');
    expect(el.textContent).toContain("You don't have permission to view this page.");
  });

  // not-found rendered its code as a <p>, so the 404 page had no h1 at all. Every page
  // needs exactly one — here the code is what names the page.
  it('renders the code as the page h1', () => {
    const { el } = render(NotFoundHost);

    const h1s = el.querySelectorAll('h1');
    expect(h1s.length).toBe(1);
    expect(h1s[0].textContent).toContain('404');
  });

  it('omits the icon unless one is asked for', () => {
    const { icon } = render(NotFoundHost);

    expect(icon()).toBeNull();
  });

  it('renders a decorative icon when given, hidden from assistive tech', () => {
    const { icon } = render(UnauthorizedHost);

    expect(icon()).not.toBeNull();
    expect(icon()!.getAttribute('aria-hidden')).toBe('true');
  });

  it('passes the testId through to the card', () => {
    const { card } = render(UnauthorizedHost);

    expect(card().getAttribute('data-testid')).toBe('error-card');
  });

  // The card had no padding at all before this component existed — hc-card puts padding
  // on its section directives, not :host, so the content sat flush against the border.
  it('gives the card padding rather than letting content touch the border', () => {
    const { card } = render(UnauthorizedHost);

    expect(getComputedStyle(card()).paddingTop).not.toBe('0px');
  });

  // Structural, not cosmetic: unauthorized renders inside the app shell (fills the
  // outlet) while not-found is routed outside it (owns the viewport).
  it('fills the main area inside the shell', () => {
    const { page } = render(UnauthorizedHost);

    expect(page().classList).toContain('centered-page--main');
  });

  it('fills the viewport outside the shell', () => {
    const { page } = render(NotFoundHost);

    expect(page().classList).toContain('centered-page--viewport');
  });

  it('offers a way back home', () => {
    const { el } = render(NotFoundHost);

    const link = el.querySelector<HTMLAnchorElement>('a[hc-button]')!;
    expect(link.textContent).toContain('Go to home');
    expect(link.getAttribute('href')).toBe('/');
  });
});

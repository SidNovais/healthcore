import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HcPagination } from './pagination';

@Component({
  imports: [HcPagination],
  template: `
    <hc-pagination
      [page]="page()"
      [pageCount]="pageCount()"
      [testId]="'orders-pagination'"
      (pageChange)="page.set($event)"
    />
  `,
})
class HostComponent {
  readonly page = signal(1);
  readonly pageCount = signal(5);
}

function render() {
  const fixture = TestBed.createComponent(HostComponent);
  fixture.detectChanges();
  const host = fixture.nativeElement as HTMLElement;
  const nav = host.querySelector<HTMLElement>('[data-testid="orders-pagination"]')!;
  const prev = host.querySelector<HTMLButtonElement>('[data-testid="orders-pagination-prev"]')!;
  const next = host.querySelector<HTMLButtonElement>('[data-testid="orders-pagination-next"]')!;
  const pageButtons = () =>
    Array.from(host.querySelectorAll<HTMLButtonElement>('[data-testid^="orders-pagination-page-"]'));
  return { fixture, host, nav, prev, next, pageButtons };
}

describe('HcPagination', () => {
  it('exposes navigation landmark semantics with an accessible label', () => {
    const { nav } = render();

    expect(nav.getAttribute('role')).toBe('navigation');
    expect(nav.getAttribute('aria-label')).toBe('pagination');
  });

  it('renders one button per page and marks the current page with aria-current', () => {
    const { pageButtons } = render();

    const buttons = pageButtons();
    expect(buttons.map(b => b.textContent?.trim())).toEqual(['1', '2', '3', '4', '5']);
    expect(buttons[0].getAttribute('aria-current')).toBe('page');
    expect(buttons[1].getAttribute('aria-current')).toBeNull();
  });

  it('emits pageChange with the clicked page number', () => {
    const { fixture, pageButtons } = render();

    pageButtons()[2].click();
    fixture.detectChanges();

    expect(fixture.componentInstance.page()).toBe(3);
    expect(pageButtons()[2].getAttribute('aria-current')).toBe('page');
  });

  it('disables prev on the first page and next on the last page', () => {
    const { fixture, prev, next } = render();

    expect(prev.disabled).toBe(true);
    expect(next.disabled).toBe(false);

    fixture.componentInstance.page.set(5);
    fixture.detectChanges();

    expect(prev.disabled).toBe(false);
    expect(next.disabled).toBe(true);
  });

  it('steps one page via prev/next', () => {
    const { fixture, prev, next } = render();

    next.click();
    fixture.detectChanges();
    expect(fixture.componentInstance.page()).toBe(2);

    prev.click();
    fixture.detectChanges();
    expect(fixture.componentInstance.page()).toBe(1);
  });

  it('collapses large ranges with an aria-hidden ellipsis around the current page', () => {
    const { fixture, host, pageButtons } = render();

    fixture.componentInstance.pageCount.set(10);
    fixture.componentInstance.page.set(5);
    fixture.detectChanges();

    expect(pageButtons().map(b => b.textContent?.trim())).toEqual(['1', '4', '5', '6', '10']);

    const ellipses = host.querySelectorAll('[data-testid="orders-pagination-ellipsis"]');
    expect(ellipses.length).toBe(2);
    expect(Array.from(ellipses).every(e => e.getAttribute('aria-hidden') === 'true')).toBe(true);
  });
});

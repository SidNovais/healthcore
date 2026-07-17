import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HcTable } from './table';

@Component({
  imports: [HcTable],
  template: `
    <table hc-table [dense]="dense()" data-testid="order-table">
      <thead>
        <tr>
          <th aria-sort="ascending"><button class="sort-btn" type="button">Patient</button></th>
          <th class="actions-col"></th>
        </tr>
      </thead>
      <tbody>
        <tr>
          <td>Maria Silva</td>
          <td><button class="row-action-trigger" type="button">…</button></td>
        </tr>
        <tr class="skeleton-row"><td colspan="2"><span class="ph"></span></td></tr>
        <tr><td class="empty-cell" colspan="2">No orders</td></tr>
      </tbody>
    </table>
  `,
})
class HostComponent {
  readonly dense = signal(false);
}

function render() {
  const fixture = TestBed.createComponent(HostComponent);
  fixture.detectChanges();
  const el = fixture.nativeElement as HTMLElement;
  const table = el.querySelector('table')!;
  const find = (sel: string) => el.querySelector<HTMLElement>(sel)!;
  return { fixture, table, find };
}

describe('HcTable', () => {
  it('applies base + tabular-nums classes and keeps the testid and content', () => {
    const { table } = render();

    expect(table.classList).toContain('hc-table');
    expect(table.classList).toContain('tabular-nums');
    expect(table.getAttribute('data-testid')).toBe('order-table');
    expect(table.querySelector('td')!.textContent).toContain('Maria Silva');
  });

  it('adds the dense modifier when dense is set', () => {
    const { fixture, table } = render();

    fixture.componentInstance.dense.set(true);
    fixture.detectChanges();

    expect(table.classList).toContain('hc-table--dense');
  });

  // These row parts were copy-pasted verbatim into every table that has them —
  // .row-action-trigger into 4 components, .actions-col into 3, .sort-btn into 2 —
  // so they now live here. They are CONSUMER-authored and merely projected, which is
  // why table.css must reach them with :host ::ng-deep: a plain scoped selector emits
  // the consumer's _ngcontent attribute and silently never matches. That exact bug
  // left every row-action menu unstyled for five phases. Assert non-token properties
  // jsdom can resolve.
  it('styles the projected row-action trigger', () => {
    const { find } = render();

    expect(getComputedStyle(find('.row-action-trigger')).display).toBe('inline-flex');
  });

  it('styles the projected sort button', () => {
    const { find } = render();

    expect(getComputedStyle(find('.sort-btn')).display).toBe('inline-flex');
  });

  it('right-aligns the projected actions column', () => {
    const { find } = render();

    expect(getComputedStyle(find('.actions-col')).textAlign).toBe('right');
  });

  it('centres the projected empty cell', () => {
    const { find } = render();

    expect(getComputedStyle(find('.empty-cell')).textAlign).toBe('center');
  });
});

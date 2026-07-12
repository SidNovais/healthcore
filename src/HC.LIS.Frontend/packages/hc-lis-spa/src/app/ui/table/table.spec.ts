import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HcTable } from './table';

@Component({
  imports: [HcTable],
  template: `
    <table hc-table [dense]="dense()" data-testid="order-table">
      <thead>
        <tr><th aria-sort="ascending">Patient</th></tr>
      </thead>
      <tbody>
        <tr><td>Maria Silva</td></tr>
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
  const table = (fixture.nativeElement as HTMLElement).querySelector('table')!;
  return { fixture, table };
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
});

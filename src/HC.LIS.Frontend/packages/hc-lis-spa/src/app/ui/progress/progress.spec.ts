import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HcProgress } from './progress';

@Component({
  imports: [HcProgress],
  template: `
    <hc-progress [value]="value()" [label]="label()" [testId]="testId()" />
    <hc-progress [indeterminate]="true" />
  `,
})
class HostComponent {
  readonly value = signal(40);
  readonly label = signal('Importing rows');
  readonly testId = signal('import-progress');
}

function render() {
  const fixture = TestBed.createComponent(HostComponent);
  fixture.detectChanges();
  return { fixture, host: fixture.nativeElement as HTMLElement };
}

describe('HcProgress', () => {
  it('exposes a determinate progressbar with clamped ARIA value', () => {
    const { host } = render();
    const bar = host.querySelector('hc-progress')!;

    expect(bar.getAttribute('role')).toBe('progressbar');
    expect(bar.getAttribute('aria-valuemin')).toBe('0');
    expect(bar.getAttribute('aria-valuemax')).toBe('100');
    expect(bar.getAttribute('aria-valuenow')).toBe('40');
    expect(bar.getAttribute('aria-label')).toBe('Importing rows');
    expect(bar.getAttribute('data-testid')).toBe('import-progress');

    const fill = bar.querySelector<HTMLElement>('.hc-progress__fill')!;
    expect(fill.style.width).toBe('40%');
  });

  it('clamps out-of-range values into 0..100', () => {
    const { fixture, host } = render();
    const component = fixture.componentInstance as HostComponent;
    const bar = host.querySelector('hc-progress')!;

    component.value.set(150);
    fixture.detectChanges();
    expect(bar.getAttribute('aria-valuenow')).toBe('100');

    component.value.set(-20);
    fixture.detectChanges();
    expect(bar.getAttribute('aria-valuenow')).toBe('0');
  });

  it('drops the ARIA value in indeterminate mode', () => {
    const { host } = render();
    const bars = Array.from(host.querySelectorAll('hc-progress'));
    const indeterminate = bars[1];

    expect(indeterminate.getAttribute('role')).toBe('progressbar');
    expect(indeterminate.hasAttribute('aria-valuenow')).toBe(false);
    expect(indeterminate.classList).toContain('hc-progress--indeterminate');
  });
});

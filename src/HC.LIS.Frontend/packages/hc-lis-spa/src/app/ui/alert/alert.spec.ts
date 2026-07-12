import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HcAlert } from './alert';

@Component({
  imports: [HcAlert],
  template: `
    <hc-alert [variant]="variant()" title="Heads up" data-testid="exam-action-error">
      Something happened.
    </hc-alert>
  `,
})
class HostComponent {
  readonly variant = signal<'info' | 'success' | 'warning' | 'error'>('info');
}

function render() {
  const fixture = TestBed.createComponent(HostComponent);
  fixture.detectChanges();
  const alert = (fixture.nativeElement as HTMLElement).querySelector('hc-alert')!;
  return { fixture, alert };
}

describe('HcAlert', () => {
  it('announces itself with role=alert and renders title + content', () => {
    const { alert } = render();

    expect(alert.getAttribute('role')).toBe('alert');
    expect(alert.classList).toContain('hc-alert');
    expect(alert.classList).toContain('hc-alert--info');
    expect(alert.querySelector('.hc-alert__title')!.textContent).toContain('Heads up');
    expect(alert.textContent).toContain('Something happened.');
  });

  it('switches variant class', () => {
    const { fixture, alert } = render();

    fixture.componentInstance.variant.set('error');
    fixture.detectChanges();

    expect(alert.classList).toContain('hc-alert--error');
  });
});

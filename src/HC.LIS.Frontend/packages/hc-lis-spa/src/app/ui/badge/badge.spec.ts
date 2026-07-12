import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HcBadge } from './badge';

@Component({
  imports: [HcBadge],
  template: `
    <hc-badge [variant]="variant()" [role]="role()" data-testid="the-badge">Requested</hc-badge>
  `,
})
class HostComponent {
  readonly variant = signal<'neutral' | 'accent' | 'success' | 'warning' | 'error'>('neutral');
  readonly role = signal<string | null>(null);
}

function render() {
  const fixture = TestBed.createComponent(HostComponent);
  fixture.detectChanges();
  const badge = (fixture.nativeElement as HTMLElement).querySelector('hc-badge')!;
  return { fixture, badge };
}

describe('HcBadge', () => {
  it('renders with the base and variant classes and projects content', () => {
    const { badge } = render();

    expect(badge.classList).toContain('hc-badge');
    expect(badge.classList).toContain('hc-badge--neutral');
    expect(badge.textContent).toContain('Requested');
  });

  it('switches to a semantic variant class', () => {
    const { fixture, badge } = render();

    fixture.componentInstance.variant.set('success');
    fixture.detectChanges();

    expect(badge.classList).toContain('hc-badge--success');
  });

  it('applies the role color hook when a user role is given', () => {
    const { fixture, badge } = render();

    fixture.componentInstance.role.set('LabTechnician');
    fixture.detectChanges();

    expect(badge.classList).toContain('hc-badge--role');
    expect(badge.getAttribute('data-role')).toBe('LabTechnician');
  });
});

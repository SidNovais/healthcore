import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HcButton } from './button';

@Component({
  imports: [HcButton],
  template: `
    <button hc-button [variant]="variant" [size]="size" [loading]="loading" data-testid="the-btn">
      Save
    </button>
  `,
})
class HostComponent {
  variant: 'default' | 'cta' | 'ghost' | 'destructive' = 'default';
  size: 'sm' | 'md' | 'icon' = 'md';
  loading = false;
}

function render() {
  const fixture = TestBed.createComponent(HostComponent);
  fixture.detectChanges();
  const button = (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>('button')!;
  return { fixture, button };
}

describe('HcButton', () => {
  it('applies base and default-variant classes to the host button', () => {
    const { button } = render();

    expect(button.classList).toContain('hc-btn');
    expect(button.classList).toContain('hc-btn--default');
    expect(button.classList).toContain('hc-btn--md');
  });

  it('keeps the data-testid on the host element', () => {
    const { button } = render();
    expect(button.getAttribute('data-testid')).toBe('the-btn');
  });

  it('switches variant class when the variant input changes', () => {
    const { fixture, button } = render();

    fixture.componentInstance.variant = 'cta';
    fixture.detectChanges();

    expect(button.classList).toContain('hc-btn--cta');
    expect(button.classList).not.toContain('hc-btn--default');
  });

  it('disables the button, marks aria-busy and shows a spinner while loading', () => {
    const { fixture, button } = render();

    fixture.componentInstance.loading = true;
    fixture.detectChanges();

    expect(button.disabled).toBe(true);
    expect(button.getAttribute('aria-busy')).toBe('true');
    expect(button.querySelector('.hc-btn__spinner')).not.toBeNull();
  });

  it('still projects its label text', () => {
    const { button } = render();
    expect(button.textContent).toContain('Save');
  });
});

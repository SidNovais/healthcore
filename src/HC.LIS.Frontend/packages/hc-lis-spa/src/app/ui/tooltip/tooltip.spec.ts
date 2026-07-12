import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HcTooltip } from './tooltip';

@Component({
  imports: [HcTooltip],
  template: `<button hcTooltip="Sign out" aria-label="Sign out">X</button>`,
})
class HostComponent {}

function render() {
  const fixture = TestBed.createComponent(HostComponent);
  fixture.detectChanges();
  const button = (fixture.nativeElement as HTMLElement).querySelector('button')!;
  return { fixture, button };
}

describe('HcTooltip', () => {
  it('shows a described tooltip on focus and hides it on blur', () => {
    const { fixture, button } = render();

    expect(document.querySelector('.hc-tooltip')).toBeNull();

    button.dispatchEvent(new Event('focus'));
    fixture.detectChanges();

    const tip = document.querySelector('.hc-tooltip')!;
    expect(tip).not.toBeNull();
    expect(tip.textContent).toContain('Sign out');
    expect(tip.getAttribute('role')).toBe('tooltip');
    expect(button.getAttribute('aria-describedby')).toBe(tip.id);

    button.dispatchEvent(new Event('blur'));
    fixture.detectChanges();

    expect(document.querySelector('.hc-tooltip')).toBeNull();
    expect(button.getAttribute('aria-describedby')).toBeNull();
  });

  it('also shows on mouseenter and hides on mouseleave', () => {
    const { fixture, button } = render();

    button.dispatchEvent(new Event('mouseenter'));
    fixture.detectChanges();
    expect(document.querySelector('.hc-tooltip')).not.toBeNull();

    button.dispatchEvent(new Event('mouseleave'));
    fixture.detectChanges();
    expect(document.querySelector('.hc-tooltip')).toBeNull();
  });

  it('removes the tooltip when the host is destroyed while visible', () => {
    const { fixture, button } = render();

    button.dispatchEvent(new Event('focus'));
    fixture.detectChanges();
    expect(document.querySelector('.hc-tooltip')).not.toBeNull();

    fixture.destroy();
    expect(document.querySelector('.hc-tooltip')).toBeNull();
  });
});

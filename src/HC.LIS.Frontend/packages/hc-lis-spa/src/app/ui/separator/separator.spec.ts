import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HcSeparator } from './separator';

@Component({
  imports: [HcSeparator],
  template: `<hc-separator /><hc-separator orientation="vertical" />`,
})
class HostComponent {}

describe('HcSeparator', () => {
  it('renders a horizontal separator by default with the separator role', () => {
    const fixture = TestBed.createComponent(HostComponent);
    fixture.detectChanges();
    const [horizontal, vertical] = Array.from(
      (fixture.nativeElement as HTMLElement).querySelectorAll('hc-separator'),
    );

    expect(horizontal.getAttribute('role')).toBe('separator');
    expect(horizontal.getAttribute('aria-orientation')).toBe('horizontal');
    expect(horizontal.classList).toContain('hc-separator');
    expect(vertical.getAttribute('aria-orientation')).toBe('vertical');
    expect(vertical.classList).toContain('hc-separator--vertical');
  });
});

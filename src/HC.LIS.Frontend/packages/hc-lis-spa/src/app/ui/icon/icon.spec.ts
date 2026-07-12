import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HcIcon } from './icon';
import type { HcIconName } from './icon';

@Component({
  imports: [HcIcon],
  template: `<hc-icon [name]="name()" [size]="size()" data-testid="the-icon" />`,
})
class HostComponent {
  readonly name = signal<HcIconName>('sun');
  readonly size = signal(16);
}

function render() {
  const fixture = TestBed.createComponent(HostComponent);
  fixture.detectChanges();
  const host = fixture.nativeElement as HTMLElement;
  return { fixture, svg: host.querySelector('svg'), host };
}

describe('HcIcon', () => {
  it('renders an inline svg with Lucide-style 1.5px stroke, hidden from AT', () => {
    const { svg } = render();

    expect(svg).not.toBeNull();
    expect(svg!.getAttribute('stroke-width')).toBe('1.5');
    expect(svg!.getAttribute('stroke')).toBe('currentColor');
    expect(svg!.getAttribute('fill')).toBe('none');
    expect(svg!.getAttribute('aria-hidden')).toBe('true');
  });

  it('sizes the svg from the size input', () => {
    const { fixture, svg } = render();

    expect(svg!.getAttribute('width')).toBe('16');

    fixture.componentInstance.size.set(20);
    fixture.detectChanges();

    expect(svg!.getAttribute('width')).toBe('20');
    expect(svg!.getAttribute('height')).toBe('20');
  });

  it('swaps the drawn path when the name changes', () => {
    const { fixture, svg } = render();
    const sunMarkup = svg!.innerHTML;

    fixture.componentInstance.name.set('moon');
    fixture.detectChanges();

    expect(svg!.innerHTML).not.toBe(sunMarkup);
  });
});

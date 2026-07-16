import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HcAvatar } from './avatar';

@Component({
  imports: [HcAvatar],
  template: `<hc-avatar [name]="name()" [src]="src()" [size]="28" testId="user-avatar" />`,
})
class HostComponent {
  readonly name = signal('Ada Lovelace');
  readonly src = signal<string | null>(null);
}

function render() {
  const fixture = TestBed.createComponent(HostComponent);
  fixture.detectChanges();
  const host = fixture.nativeElement as HTMLElement;
  return {
    fixture,
    component: fixture.componentInstance,
    root: () => host.querySelector<HTMLElement>('[data-testid="user-avatar"]')!,
    fallback: () => host.querySelector<HTMLElement>('.hc-avatar__fallback'),
    img: () => host.querySelector<HTMLImageElement>('img'),
  };
}

describe('HcAvatar', () => {
  it('derives two-letter initials from a first and last name', () => {
    const { fallback } = render();

    expect(fallback()?.textContent?.trim()).toBe('AL');
  });

  it('uses the first two characters when the name is a single word', () => {
    const { fixture, component, fallback } = render();

    component.name.set('itadmin@hclis.local');
    fixture.detectChanges();

    expect(fallback()?.textContent?.trim()).toBe('IT');
  });

  it('renders the image with the name as alt text when a src is given', () => {
    const { fixture, component, img, fallback } = render();

    component.src.set('https://example.test/ada.png');
    fixture.detectChanges();

    expect(img()?.getAttribute('src')).toBe('https://example.test/ada.png');
    expect(img()?.getAttribute('alt')).toBe('Ada Lovelace');
    expect(fallback()).toBeNull();
  });

  it('falls back to initials when the image fails to load', () => {
    const { fixture, component, img, fallback } = render();

    component.src.set('https://example.test/broken.png');
    fixture.detectChanges();
    img()!.dispatchEvent(new Event('error'));
    fixture.detectChanges();

    expect(img()).toBeNull();
    expect(fallback()?.textContent?.trim()).toBe('AL');
  });

  it('sizes the host and hides the decorative fallback from assistive tech', () => {
    const { root, fallback } = render();

    expect(root().style.getPropertyValue('--hc-avatar-size')).toBe('28px');
    expect(fallback()?.getAttribute('aria-hidden')).toBe('true');
  });
});

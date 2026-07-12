import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HcSkeleton } from './skeleton';
import { HcSpinner } from './spinner';

@Component({
  imports: [HcSkeleton, HcSpinner],
  template: `
    <hc-skeleton style="width: 8rem; height: 1rem" />
    <hc-spinner label="Loading orders" />
    <hc-spinner />
  `,
})
class HostComponent {}

function render() {
  const fixture = TestBed.createComponent(HostComponent);
  fixture.detectChanges();
  return fixture.nativeElement as HTMLElement;
}

describe('HcSkeleton', () => {
  it('is a purely decorative placeholder hidden from AT', () => {
    const host = render();
    const skeleton = host.querySelector('hc-skeleton')!;

    expect(skeleton.classList).toContain('hc-skeleton');
    expect(skeleton.getAttribute('aria-hidden')).toBe('true');
  });
});

describe('HcSpinner', () => {
  it('announces as status with a configurable label', () => {
    const host = render();
    const [labeled, unlabeled] = Array.from(host.querySelectorAll('hc-spinner'));

    expect(labeled.getAttribute('role')).toBe('status');
    expect(labeled.getAttribute('aria-label')).toBe('Loading orders');
    expect(labeled.classList).toContain('hc-spinner');
    expect(unlabeled.getAttribute('aria-label')).toBe('Loading');
  });
});

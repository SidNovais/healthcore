import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HcCard, HcCardContent, HcCardFooter, HcCardHeader, HcCardTitle } from './card';

@Component({
  imports: [HcCard, HcCardHeader, HcCardTitle, HcCardContent, HcCardFooter],
  template: `
    <hc-card data-testid="the-card">
      <div hc-card-header>
        <h3 hc-card-title>Patient</h3>
      </div>
      <div hc-card-content>Body</div>
      <div hc-card-footer>Footer</div>
    </hc-card>
  `,
})
class HostComponent {}

describe('HcCard', () => {
  it('applies the card classes to host and sections', () => {
    const fixture = TestBed.createComponent(HostComponent);
    fixture.detectChanges();
    const host = fixture.nativeElement as HTMLElement;

    expect(host.querySelector('hc-card')!.classList).toContain('hc-card');
    expect(host.querySelector('[hc-card-header]')!.classList).toContain('hc-card__header');
    expect(host.querySelector('[hc-card-title]')!.classList).toContain('hc-card__title');
    expect(host.querySelector('[hc-card-content]')!.classList).toContain('hc-card__content');
    expect(host.querySelector('[hc-card-footer]')!.classList).toContain('hc-card__footer');
    expect(host.querySelector('hc-card')!.getAttribute('data-testid')).toBe('the-card');
  });
});

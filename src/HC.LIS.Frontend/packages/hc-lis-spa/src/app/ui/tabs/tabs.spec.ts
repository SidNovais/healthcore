import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HcTab, HcTabPanel, HcTabs } from './tabs';

@Component({
  imports: [HcTabs, HcTab, HcTabPanel],
  template: `
    <hc-tabs [(value)]="active">
      <button hc-tab value="all" data-testid="filter-tab-all">All</button>
      <button hc-tab value="waiting" data-testid="filter-tab-waiting">Waiting</button>
      <hc-tab-panel value="all">All content</hc-tab-panel>
      <hc-tab-panel value="waiting">Waiting content</hc-tab-panel>
    </hc-tabs>
  `,
})
class HostComponent {
  readonly active = signal('all');
}

function render() {
  const fixture = TestBed.createComponent(HostComponent);
  fixture.detectChanges();
  const host = fixture.nativeElement as HTMLElement;
  const tabs = Array.from(host.querySelectorAll<HTMLButtonElement>('[hc-tab]'));
  const panels = Array.from(host.querySelectorAll<HTMLElement>('hc-tab-panel'));
  return { fixture, host, tabs, panels };
}

describe('HcTabs', () => {
  it('wires the tablist/tab/tabpanel roles', () => {
    const { host, tabs, panels } = render();

    expect(host.querySelector('[role="tablist"]')).not.toBeNull();
    expect(tabs.every(t => t.getAttribute('role') === 'tab')).toBe(true);
    expect(panels.every(p => p.getAttribute('role') === 'tabpanel')).toBe(true);
  });

  it('marks the active tab selected and shows only its panel', () => {
    const { tabs, panels } = render();

    expect(tabs[0].getAttribute('aria-selected')).toBe('true');
    expect(tabs[1].getAttribute('aria-selected')).toBe('false');
    expect(panels[0].hasAttribute('hidden')).toBe(false);
    expect(panels[1].hasAttribute('hidden')).toBe(true);
  });

  it('switches tab on click and updates the two-way model', () => {
    const { fixture, tabs, panels } = render();

    tabs[1].click();
    fixture.detectChanges();

    expect(fixture.componentInstance.active()).toBe('waiting');
    expect(tabs[1].getAttribute('aria-selected')).toBe('true');
    expect(panels[1].hasAttribute('hidden')).toBe(false);
    expect(panels[0].hasAttribute('hidden')).toBe(true);
  });

  it('moves selection with arrow keys (roving focus)', () => {
    const { fixture, tabs } = render();

    tabs[0].dispatchEvent(new KeyboardEvent('keydown', { key: 'ArrowRight', bubbles: true }));
    fixture.detectChanges();

    expect(fixture.componentInstance.active()).toBe('waiting');
    expect(tabs[1].tabIndex).toBe(0);
    expect(tabs[0].tabIndex).toBe(-1);
  });
});

import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HcDropdownMenu, HcDropdownMenuItem, HcDropdownMenuTrigger } from './dropdown-menu';

@Component({
  imports: [HcDropdownMenu, HcDropdownMenuTrigger, HcDropdownMenuItem],
  template: `
    <button data-testid="outside">outside</button>
    <hc-dropdown-menu testId="row-actions">
      <button hc-dropdown-trigger data-testid="row-actions-trigger">Actions</button>
      <button hc-dropdown-item data-testid="action-view" (select)="picked.set('view')">View</button>
      <button hc-dropdown-item data-testid="action-accept" (select)="picked.set('accept')">
        Accept
      </button>
    </hc-dropdown-menu>
  `,
})
class HostComponent {
  readonly picked = signal<string | null>(null);
}

function render() {
  const fixture = TestBed.createComponent(HostComponent);
  fixture.detectChanges();
  const host = fixture.nativeElement as HTMLElement;
  document.body.appendChild(host); // needed so element.focus() / activeElement work
  const trigger = host.querySelector<HTMLButtonElement>('[data-testid="row-actions-trigger"]')!;
  const menu = () => host.querySelector<HTMLElement>('[role="menu"]');
  const items = () => Array.from(host.querySelectorAll<HTMLButtonElement>('[hc-dropdown-item]'));
  const open = () => {
    trigger.click();
    fixture.detectChanges();
  };
  return { fixture, host, trigger, menu, items, open };
}

function keydown(el: HTMLElement, key: string) {
  el.dispatchEvent(new KeyboardEvent('keydown', { key, bubbles: true }));
}

describe('HcDropdownMenu', () => {
  it('wires the trigger popup semantics and keeps the menu closed initially', () => {
    const { trigger, menu } = render();

    expect(trigger.getAttribute('aria-haspopup')).toBe('menu');
    expect(trigger.getAttribute('aria-expanded')).toBe('false');
    expect(menu()).toBeNull();
  });

  it('opens the menu on trigger click with menu/menuitem roles', () => {
    const { trigger, menu, items, open } = render();

    open();

    expect(trigger.getAttribute('aria-expanded')).toBe('true');
    expect(menu()).not.toBeNull();
    expect(items().every(i => i.getAttribute('role') === 'menuitem')).toBe(true);
  });

  it('emits the item select output and closes the menu on activation', () => {
    const { fixture, items, menu, open } = render();

    open();
    items()[0].click();
    fixture.detectChanges();

    expect(fixture.componentInstance.picked()).toBe('view');
    expect(menu()).toBeNull();
  });

  it('moves focus between items with arrow keys and wraps', () => {
    const { items, open } = render();

    open();
    items()[0].focus();

    keydown(items()[0], 'ArrowDown');
    expect(document.activeElement).toBe(items()[1]);

    keydown(items()[1], 'ArrowDown');
    expect(document.activeElement).toBe(items()[0]);

    keydown(items()[0], 'ArrowUp');
    expect(document.activeElement).toBe(items()[1]);
  });

  it('closes on Escape and returns focus to the trigger', () => {
    const { fixture, trigger, items, menu, open } = render();

    open();
    items()[0].focus();
    keydown(items()[0], 'Escape');
    fixture.detectChanges();

    expect(menu()).toBeNull();
    expect(document.activeElement).toBe(trigger);
  });

  // Menu items are declared in the *consumer's* template and content-projected, so under
  // emulated encapsulation they carry the consumer's _ngcontent attribute — not this
  // component's. Item styling must therefore pierce encapsulation to reach them.
  it('styles content-projected menu items', () => {
    const { items, open } = render();

    open();

    expect(getComputedStyle(items()[0]).display).toBe('flex');
  });

  it('closes when a click lands outside the menu', () => {
    const { fixture, host, menu, open } = render();

    open();
    host.querySelector<HTMLButtonElement>('[data-testid="outside"]')!.click();
    fixture.detectChanges();

    expect(menu()).toBeNull();
  });

  // Regression: gsap.from() renders its start state immediately, and autoAlpha:0 sets
  // visibility:hidden — which blurs whatever the menu just focused. openAndFocusFirst()
  // (the ArrowDown path) focuses items()[0], then the entrance tween hid the panel and
  // dropped focus to <body>, leaving the menu unusable by keyboard. Traced in a real
  // browser on hc-command; this primitive had the identical ordering. The entrance may
  // fade (opacity) but must never hide. jsdom does not blur on visibility:hidden, so
  // assert the invariant that causes it.
  it('does not hide the menu while animating in (hiding it blurs the focused item)', async () => {
    const { fixture, menu, open } = render();

    open();
    await fixture.whenStable();

    expect(menu()!.style.visibility).not.toBe('hidden');
  });
});

import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HcCommand, type HcCommandItem } from './command';

@Component({
  imports: [HcCommand],
  template: `
    <hc-command
      [(open)]="open"
      [(query)]="query"
      [items]="items()"
      testId="palette"
      (select)="picked.set($event.id)"
    />
  `,
})
class HostComponent {
  readonly open = signal(false);
  readonly query = signal('');
  readonly items = signal<HcCommandItem[]>([
    { id: 'nav-orders', label: 'Orders', group: 'Navigation' },
    { id: 'nav-patients', label: 'Patients', group: 'Navigation' },
    { id: 'p-1', label: 'Ada Lovelace', group: 'Patients', hint: 'DOB 1815-12-10' },
  ]);
  readonly picked = signal<string | null>(null);
}

function render() {
  const fixture = TestBed.createComponent(HostComponent);
  fixture.detectChanges();
  const host = fixture.nativeElement as HTMLElement;
  document.body.appendChild(host); // needed so element.focus() / activeElement work

  const api = {
    fixture,
    component: fixture.componentInstance,
    input: () => host.querySelector<HTMLInputElement>('[data-testid="palette-input"]'),
    list: () => host.querySelector<HTMLElement>('[data-testid="palette-list"]'),
    options: () => Array.from(host.querySelectorAll<HTMLElement>('[role="option"]')),
    groups: () => Array.from(host.querySelectorAll<HTMLElement>('[role="group"]')),
    empty: () => host.querySelector<HTMLElement>('[data-testid="palette-empty"]'),
    active: () => host.querySelector<HTMLElement>('[role="option"][aria-selected="true"]'),
    open: () => {
      fixture.componentInstance.open.set(true);
      fixture.detectChanges();
    },
    key: (k: string) => {
      api.input()!.dispatchEvent(new KeyboardEvent('keydown', { key: k, bubbles: true }));
      fixture.detectChanges();
    },
  };
  return api;
}

describe('HcCommand', () => {
  it('renders no palette content until it is opened', () => {
    const { input, options } = render();

    expect(input()).toBeNull();
    expect(options()).toHaveLength(0);
  });

  it('wires the combobox contract onto the input', () => {
    const { open, input, list } = render();

    open();

    expect(input()!.getAttribute('role')).toBe('combobox');
    expect(input()!.getAttribute('aria-expanded')).toBe('true');
    expect(input()!.getAttribute('aria-autocomplete')).toBe('list');
    expect(input()!.getAttribute('aria-controls')).toBe(list()!.id);
    expect(list()!.getAttribute('role')).toBe('listbox');
  });

  it('renders the items as options under their group headings', () => {
    const { open, options, groups } = render();

    open();

    expect(options()).toHaveLength(3);
    expect(groups()).toHaveLength(2);
    expect(groups()[0].textContent).toContain('Navigation');
    expect(groups()[1].textContent).toContain('Patients');
  });

  it('activates the first option and points aria-activedescendant at it', () => {
    const { open, input, active } = render();

    open();

    expect(active()?.textContent).toContain('Orders');
    expect(input()!.getAttribute('aria-activedescendant')).toBe(active()!.id);
  });

  it('moves the active option with the arrow keys, wrapping at both ends', () => {
    const { open, key, active } = render();

    open();

    key('ArrowDown');
    expect(active()?.textContent).toContain('Patients');

    key('ArrowDown');
    expect(active()?.textContent).toContain('Ada Lovelace');

    key('ArrowDown');
    expect(active()?.textContent).toContain('Orders');

    key('ArrowUp');
    expect(active()?.textContent).toContain('Ada Lovelace');
  });

  it('selects the active option on Enter and closes', () => {
    const { open, key, component } = render();

    open();
    key('ArrowDown');
    key('Enter');

    expect(component.picked()).toBe('nav-patients');
    expect(component.open()).toBe(false);
  });

  it('selects an option on click', () => {
    const { open, options, component, fixture } = render();

    open();
    options()[2].click();
    fixture.detectChanges();

    expect(component.picked()).toBe('p-1');
    expect(component.open()).toBe(false);
  });

  it('two-way binds the typed query', () => {
    const { open, input, fixture, component } = render();

    open();
    input()!.value = 'ada';
    input()!.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    expect(component.query()).toBe('ada');
  });

  it('shows the empty message when there is nothing to show', () => {
    const { open, component, fixture, empty, options } = render();

    open();
    component.items.set([]);
    fixture.detectChanges();

    expect(options()).toHaveLength(0);
    expect(empty()).not.toBeNull();
  });

  it('re-activates the first option when the item list changes', () => {
    const { open, key, component, fixture, active } = render();

    open();
    key('ArrowDown');
    expect(active()?.textContent).toContain('Patients');

    component.items.set([{ id: 'p-9', label: 'Grace Hopper', group: 'Patients' }]);
    fixture.detectChanges();

    expect(active()?.textContent).toContain('Grace Hopper');
  });
});

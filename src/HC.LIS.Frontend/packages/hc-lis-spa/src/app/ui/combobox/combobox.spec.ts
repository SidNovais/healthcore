import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HcCombobox, type HcComboboxOption } from './combobox';

@Component({
  imports: [HcCombobox],
  template: `
    <hc-combobox
      [options]="options()"
      placeholder="Search patients"
      data-testid="patient-picker"
      (queryChange)="lastQuery = $event"
      (selected)="lastSelected = $event"
    />
  `,
})
class HostComponent {
  readonly options = signal<readonly HcComboboxOption[]>([]);
  lastQuery = '';
  lastSelected: HcComboboxOption | null = null;
}

function render() {
  const fixture = TestBed.createComponent(HostComponent);
  fixture.detectChanges();
  const host = fixture.nativeElement as HTMLElement;
  const input = host.querySelector<HTMLInputElement>('input')!;
  return { fixture, host, input };
}

function type(input: HTMLInputElement, text: string): void {
  input.value = text;
  input.dispatchEvent(new Event('input', { bubbles: true }));
}

const PATIENTS: readonly HcComboboxOption[] = [
  { value: 'p-1', label: 'Maria Silva' },
  { value: 'p-2', label: 'Marcos Souza' },
];

describe('HcCombobox', () => {
  it('renders a collapsed combobox input', () => {
    const { input } = render();

    expect(input.getAttribute('role')).toBe('combobox');
    expect(input.getAttribute('aria-expanded')).toBe('false');
    expect(input.getAttribute('aria-autocomplete')).toBe('list');
    expect(input.placeholder).toBe('Search patients');
  });

  it('emits queryChange as the user types and expands when options arrive', () => {
    const { fixture, host, input } = render();

    type(input, 'Mar');
    fixture.detectChanges();
    expect(fixture.componentInstance.lastQuery).toBe('Mar');

    fixture.componentInstance.options.set(PATIENTS);
    fixture.detectChanges();

    expect(input.getAttribute('aria-expanded')).toBe('true');
    const listbox = host.querySelector('[role="listbox"]')!;
    expect(listbox).not.toBeNull();
    expect(listbox.querySelectorAll('[role="option"]')).toHaveLength(2);
  });

  it('selects an option on click, emits it, and collapses', () => {
    const { fixture, host, input } = render();

    type(input, 'Mar');
    fixture.componentInstance.options.set(PATIENTS);
    fixture.detectChanges();

    host.querySelectorAll<HTMLElement>('[role="option"]')[1].click();
    fixture.detectChanges();

    expect(fixture.componentInstance.lastSelected).toEqual(PATIENTS[1]);
    expect(input.getAttribute('aria-expanded')).toBe('false');
    expect(host.querySelector('[role="listbox"]')).toBeNull();
  });

  it('supports ArrowDown + Enter keyboard selection', () => {
    const { fixture, host, input } = render();

    type(input, 'Mar');
    fixture.componentInstance.options.set(PATIENTS);
    fixture.detectChanges();

    input.dispatchEvent(new KeyboardEvent('keydown', { key: 'ArrowDown', bubbles: true }));
    fixture.detectChanges();
    const active = host.querySelector('[role="option"].hc-combobox__option--active')!;
    expect(active.textContent).toContain('Maria Silva');
    expect(input.getAttribute('aria-activedescendant')).toBe(active.id);

    input.dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter', bubbles: true }));
    fixture.detectChanges();

    expect(fixture.componentInstance.lastSelected).toEqual(PATIENTS[0]);
  });

  it('closes the listbox on Escape without selecting', () => {
    const { fixture, host, input } = render();

    type(input, 'Mar');
    fixture.componentInstance.options.set(PATIENTS);
    fixture.detectChanges();

    input.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }));
    fixture.detectChanges();

    expect(host.querySelector('[role="listbox"]')).toBeNull();
    expect(fixture.componentInstance.lastSelected).toBeNull();
  });
});

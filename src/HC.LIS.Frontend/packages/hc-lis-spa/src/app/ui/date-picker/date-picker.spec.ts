import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { HcDatePicker } from './date-picker';

@Component({
  imports: [ReactiveFormsModule, HcDatePicker],
  template: `
    <button data-testid="outside">outside</button>
    <hc-date-picker
      testId="dob"
      inputTestId="patient-dob-input"
      inputId="patient-dob"
      [max]="max()"
      [formControl]="control"
    />
  `,
})
class HostComponent {
  readonly control = new FormControl('');
  readonly max = signal<string | null>(null);
}

function render() {
  const fixture = TestBed.createComponent(HostComponent);
  fixture.detectChanges();
  const host = fixture.nativeElement as HTMLElement;
  // Needed so element.focus() / document.activeElement behave.
  document.body.appendChild(host);

  const testId = (id: string) => host.querySelector<HTMLElement>(`[data-testid="${id}"]`);
  const input = () => testId('patient-dob-input') as HTMLInputElement;
  const trigger = () => testId('dob-trigger') as HTMLButtonElement;
  const calendar = () => testId('dob-calendar');
  const days = () => Array.from(host.querySelectorAll<HTMLButtonElement>('[data-hc-day]'));
  const day = (iso: string) => testId(`dob-day-${iso}`) as HTMLButtonElement | null;

  const detect = () => fixture.detectChanges();
  const open = () => {
    trigger().click();
    detect();
  };
  const type = (value: string) => {
    input().value = value;
    input().dispatchEvent(new Event('input'));
    detect();
  };

  return { fixture, host, testId, input, trigger, calendar, days, day, detect, open, type };
}

function keydown(el: HTMLElement, key: string) {
  el.dispatchEvent(new KeyboardEvent('keydown', { key, bubbles: true }));
}

describe('HcDatePicker', () => {
  afterEach(() => {
    document.body.replaceChildren();
  });

  it('renders a text input carrying the consumer testid and id, with the calendar closed', () => {
    const { input, trigger, calendar } = render();

    expect(input()).not.toBeNull();
    // A text input (not type=date) so the popover is the only calendar UI, but
    // Playwright's .fill('1990-01-15') on the same testid keeps working.
    expect(input().type).toBe('text');
    expect(input().id).toBe('patient-dob');
    expect(trigger().getAttribute('aria-haspopup')).toBe('dialog');
    expect(trigger().getAttribute('aria-expanded')).toBe('false');
    expect(calendar()).toBeNull();
  });

  it('renders the form control value in the input', () => {
    const { fixture, input, detect } = render();

    fixture.componentInstance.control.setValue('1990-01-15');
    detect();

    expect(input().value).toBe('1990-01-15');
  });

  it('writes a typed ISO date back to the form control', () => {
    const { fixture, type } = render();

    type('1990-01-15');

    expect(fixture.componentInstance.control.value).toBe('1990-01-15');
  });

  it('marks the control touched when the input is blurred', () => {
    const { fixture, input, detect } = render();

    input().dispatchEvent(new Event('blur'));
    detect();

    expect(fixture.componentInstance.control.touched).toBe(true);
  });

  it('opens a labelled dialog calendar from the trigger', () => {
    const { trigger, calendar, open } = render();

    open();

    expect(trigger().getAttribute('aria-expanded')).toBe('true');
    expect(calendar()).not.toBeNull();
    expect(calendar()!.getAttribute('role')).toBe('dialog');
    expect(calendar()!.getAttribute('aria-label')).toBeTruthy();
  });

  it('renders the selected month as a grid and marks the selected day', () => {
    const { fixture, day, days, detect, open } = render();

    fixture.componentInstance.control.setValue('1990-01-15');
    detect();
    open();

    // January 1990 has 31 days.
    expect(days()).toHaveLength(31);
    expect(day('1990-01-15')!.getAttribute('aria-selected')).toBe('true');
    expect(day('1990-01-14')!.getAttribute('aria-selected')).toBe('false');
  });

  it('selects a day, closes the calendar and returns focus to the trigger', () => {
    const { fixture, day, calendar, trigger, detect, open } = render();

    fixture.componentInstance.control.setValue('1990-01-15');
    detect();
    open();
    day('1990-01-22')!.click();
    detect();

    expect(fixture.componentInstance.control.value).toBe('1990-01-22');
    expect(calendar()).toBeNull();
    expect(document.activeElement).toBe(trigger());
  });

  it('jumps years from the year select — the control that makes a calendar usable for a birthdate', () => {
    const { fixture, testId, day, detect, open } = render();

    fixture.componentInstance.control.setValue('1990-01-15');
    detect();
    open();

    const year = testId('dob-year') as HTMLSelectElement;
    year.value = '1975';
    year.dispatchEvent(new Event('change'));
    detect();

    expect(day('1975-01-15')).not.toBeNull();
    expect(day('1990-01-15')).toBeNull();
  });

  it('pages months with the previous/next buttons', () => {
    const { fixture, testId, day, detect, open } = render();

    fixture.componentInstance.control.setValue('1990-01-15');
    detect();
    open();

    testId('dob-next-month')!.click();
    detect();
    expect(day('1990-02-15')).not.toBeNull();

    testId('dob-prev-month')!.click();
    testId('dob-prev-month')!.click();
    detect();
    expect(day('1989-12-15')).not.toBeNull();
  });

  it('disables days after max so a birthdate cannot be set in the future', () => {
    const { fixture, day, detect, open } = render();

    fixture.componentInstance.max.set('1990-01-20');
    fixture.componentInstance.control.setValue('1990-01-15');
    detect();
    open();

    expect(day('1990-01-20')!.disabled).toBe(false);
    expect(day('1990-01-21')!.disabled).toBe(true);
  });

  it('moves the active day with arrow keys and selects it with Enter', () => {
    const { fixture, calendar, detect, open } = render();

    fixture.componentInstance.control.setValue('1990-01-15');
    detect();
    open();

    keydown(calendar()!, 'ArrowRight');
    detect();
    keydown(calendar()!, 'ArrowDown');
    detect();
    keydown(calendar()!, 'Enter');
    detect();

    // +1 day, then +7 days.
    expect(fixture.componentInstance.control.value).toBe('1990-01-23');
  });

  it('closes on Escape and returns focus to the trigger', () => {
    const { calendar, trigger, detect, open } = render();

    open();
    keydown(calendar()!, 'Escape');
    detect();

    expect(calendar()).toBeNull();
    expect(document.activeElement).toBe(trigger());
  });

  it('closes when clicking outside', () => {
    const { testId, calendar, detect, open } = render();

    open();
    testId('outside')!.click();
    detect();

    expect(calendar()).toBeNull();
  });

  it('parses an ISO value without drifting across timezones', () => {
    const { fixture, day, detect, open } = render();

    // new Date('1990-01-01') parses as UTC midnight — in any negative-offset zone
    // that renders as Dec 31. The picker must do calendar math on Y/M/D parts.
    fixture.componentInstance.control.setValue('1990-01-01');
    detect();
    open();

    expect(day('1990-01-01')!.getAttribute('aria-selected')).toBe('true');
  });

  // Regression: gsap.from() renders its start state immediately, and autoAlpha:0 sets
  // visibility:hidden — which blurs the calendar it had just focused a line earlier,
  // breaking arrow-key date navigation (the grid owns focus + aria-activedescendant).
  // Traced in a real browser on hc-command; this primitive had the identical ordering.
  // The entrance may fade (opacity) but must never hide. jsdom does not blur on
  // visibility:hidden, so assert the invariant that causes it.
  it('does not hide the calendar while animating in (hiding it blurs the grid)', async () => {
    const { fixture, calendar, open } = render();

    open();
    await fixture.whenStable();

    expect(calendar()!.style.visibility).not.toBe('hidden');
  });
});

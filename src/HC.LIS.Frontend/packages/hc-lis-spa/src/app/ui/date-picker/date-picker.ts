import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  HostListener,
  computed,
  forwardRef,
  inject,
  input,
  signal,
  viewChild,
} from '@angular/core';
import { NG_VALUE_ACCESSOR, type ControlValueAccessor } from '@angular/forms';
import { gsap } from 'gsap';
import { HcIcon } from '../icon/icon';
import { MOTION, prefersReducedMotion } from '../motion/motion';

/** A calendar date as its parts. `m` is 1-12 — not the 0-11 a Date carries. */
interface Ymd {
  y: number;
  m: number;
  d: number;
}

const MONTHS = [
  'January',
  'February',
  'March',
  'April',
  'May',
  'June',
  'July',
  'August',
  'September',
  'October',
  'November',
  'December',
];

/** Sunday-first, matching getDay(). `abbr` names the column for screen readers. */
const WEEKDAYS = [
  { abbr: 'Sunday', short: 'Su' },
  { abbr: 'Monday', short: 'Mo' },
  { abbr: 'Tuesday', short: 'Tu' },
  { abbr: 'Wednesday', short: 'We' },
  { abbr: 'Thursday', short: 'Th' },
  { abbr: 'Friday', short: 'Fr' },
  { abbr: 'Saturday', short: 'Sa' },
];

/** How far back the year select reaches — a lifetime, since this picks birthdates. */
const YEAR_SPAN = 120;

const ISO_PATTERN = /^(\d{4})-(\d{2})-(\d{2})$/;

/**
 * Parses `YYYY-MM-DD` into calendar parts, rejecting non-dates like `1990-02-31`.
 *
 * Deliberately string-based: `new Date('1990-01-01')` parses as *UTC* midnight, so
 * in any negative-offset timezone it reads back as Dec 31. Every date in this
 * component is therefore handled as Y/M/D parts, never as an instant.
 */
function parseIso(value: string): Ymd | null {
  const match = ISO_PATTERN.exec(value);
  if (!match) {
    return null;
  }
  const [y, m, d] = [Number(match[1]), Number(match[2]), Number(match[3])];
  const probe = new Date(y, m - 1, d);
  const isReal = probe.getFullYear() === y && probe.getMonth() === m - 1 && probe.getDate() === d;
  return isReal ? { y, m, d } : null;
}

function toIso({ y, m, d }: Ymd): string {
  return `${String(y).padStart(4, '0')}-${String(m).padStart(2, '0')}-${String(d).padStart(2, '0')}`;
}

function todayYmd(): Ymd {
  const now = new Date();
  return { y: now.getFullYear(), m: now.getMonth() + 1, d: now.getDate() };
}

function daysInMonth(y: number, m: number): number {
  // Day 0 of the next month is the last day of this one.
  return new Date(y, m, 0).getDate();
}

function addDays(ymd: Ymd, delta: number): Ymd {
  const shifted = new Date(ymd.y, ymd.m - 1, ymd.d + delta);
  return { y: shifted.getFullYear(), m: shifted.getMonth() + 1, d: shifted.getDate() };
}

/** Calendar-order comparison; `null` sorts as unbounded. */
function isAfter(a: Ymd, b: Ymd): boolean {
  return toIso(a) > toIso(b);
}

let nextId = 0;

/**
 * Date picker blueprinted from the shadcn *date-picker-with-input* anatomy: the
 * text input stays the primary control and the calendar popover is an affordance
 * beside it.
 *
 * That split is deliberate. A calendar-only trigger is a poor way to enter a
 * birthdate — it forces month-by-month paging through decades — and it would
 * break every caller that types a date. Typing `1990-01-15` remains the fast path;
 * the popover exists for browsing, and leads with a year select for the same reason.
 *
 * The value is an ISO `YYYY-MM-DD` string, matching the API contract, and is
 * exposed through ControlValueAccessor so consumers bind `formControlName`.
 *
 * a11y follows the dialog-plus-grid pattern: focus rests on the calendar container
 * and `aria-activedescendant` tracks the active day, so the highlight is painted
 * explicitly rather than via `:focus` (as `hc-command` does).
 */
@Component({
  selector: 'hc-date-picker',
  imports: [HcIcon],
  templateUrl: './date-picker.html',
  styleUrl: './date-picker.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => HcDatePicker),
      multi: true,
    },
  ],
})
export class HcDatePicker implements ControlValueAccessor {
  readonly testId = input('date-picker');
  readonly inputId = input<string | null>(null);
  readonly inputTestId = input<string | null>(null);
  /** Latest selectable date as ISO; days after it are disabled. */
  readonly max = input<string | null>(null);
  readonly invalid = input(false);
  readonly placeholder = input('YYYY-MM-DD');
  readonly ariaLabel = input('Choose date');

  private readonly host = inject(ElementRef<HTMLElement>);
  private readonly uid = nextId++;

  protected readonly value = signal('');
  protected readonly open = signal(false);
  protected readonly disabled = signal(false);

  private readonly view = signal<{ y: number; m: number }>({ y: todayYmd().y, m: todayYmd().m });
  protected readonly activeDay = signal<Ymd>(todayYmd());

  private readonly triggerRef = viewChild<ElementRef<HTMLButtonElement>>('trigger');
  private readonly calendarRef = viewChild<ElementRef<HTMLElement>>('calendar');

  protected readonly months = MONTHS;
  protected readonly weekdays = WEEKDAYS;

  protected readonly resolvedInputTestId = computed(
    () => this.inputTestId() ?? `${this.testId()}-input`,
  );

  private readonly maxYmd = computed(() => {
    const raw = this.max();
    return raw ? parseIso(raw) : null;
  });

  protected readonly selected = computed(() => parseIso(this.value()));

  protected readonly viewMonth = computed(() => this.view().m);
  protected readonly viewYear = computed(() => this.view().y);

  protected readonly years = computed(() => {
    const last = this.maxYmd()?.y ?? todayYmd().y;
    const first = last - YEAR_SPAN;
    // Newest first: a birthdate is far likelier to be recent than 120 years back.
    return Array.from({ length: YEAR_SPAN + 1 }, (_, i) => last - i).filter(y => y >= first);
  });

  protected readonly monthLabel = computed(() => `${MONTHS[this.view().m - 1]} ${this.view().y}`);

  /** Leading blanks so the 1st lands under its weekday column. */
  protected readonly leadingBlanks = computed(() => {
    const { y, m } = this.view();
    return Array.from({ length: new Date(y, m - 1, 1).getDay() }, (_, i) => i);
  });

  protected readonly days = computed<Ymd[]>(() => {
    const { y, m } = this.view();
    return Array.from({ length: daysInMonth(y, m) }, (_, i) => ({ y, m, d: i + 1 }));
  });

  protected readonly activeId = computed(() => this.dayId(this.activeDay()));

  private onChange: (value: string) => void = () => {};
  private onTouched: () => void = () => {};

  writeValue(value: string | null): void {
    this.value.set(value ?? '');
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled.set(isDisabled);
  }

  protected dayId(ymd: Ymd): string {
    return `hc-date-picker-${this.uid}-${toIso(ymd)}`;
  }

  protected dayTestId(ymd: Ymd): string {
    return `${this.testId()}-day-${toIso(ymd)}`;
  }

  protected dayLabel(ymd: Ymd): string {
    return `${MONTHS[ymd.m - 1]} ${ymd.d}, ${ymd.y}`;
  }

  protected isSelected(ymd: Ymd): boolean {
    const selected = this.selected();
    return !!selected && toIso(selected) === toIso(ymd);
  }

  protected isActive(ymd: Ymd): boolean {
    return toIso(this.activeDay()) === toIso(ymd);
  }

  protected isToday(ymd: Ymd): boolean {
    return toIso(todayYmd()) === toIso(ymd);
  }

  protected isDisabled(ymd: Ymd): boolean {
    const max = this.maxYmd();
    return !!max && isAfter(ymd, max);
  }

  protected onInput(raw: string): void {
    this.value.set(raw);
    this.onChange(raw);
  }

  protected onBlur(): void {
    this.onTouched();
  }

  protected toggle(): void {
    if (this.open()) {
      this.close();
      return;
    }
    // Anchor the view on the current value, falling back to today (or max, when
    // today is out of bounds) so the popover opens somewhere useful.
    const max = this.maxYmd();
    const today = todayYmd();
    const fallback = max && isAfter(today, max) ? max : today;
    const anchor = this.selected() ?? fallback;
    this.view.set({ y: anchor.y, m: anchor.m });
    this.activeDay.set(anchor);
    this.open.set(true);
    queueMicrotask(() => {
      const calendar = this.calendarRef()?.nativeElement;
      calendar?.focus();
      if (calendar && !prefersReducedMotion()) {
        gsap.from(calendar, { autoAlpha: 0, y: -4, duration: MOTION.fast, overwrite: true });
      }
    });
  }

  protected close(returnFocus = true): void {
    if (!this.open()) {
      return;
    }
    this.open.set(false);
    if (returnFocus) {
      this.triggerRef()?.nativeElement.focus();
    }
  }

  protected select(ymd: Ymd): void {
    if (this.isDisabled(ymd)) {
      return;
    }
    const iso = toIso(ymd);
    this.value.set(iso);
    this.onChange(iso);
    this.onTouched();
    this.close();
  }

  protected onYearChange(raw: string): void {
    this.setView(Number(raw), this.view().m);
  }

  protected onMonthChange(raw: string): void {
    this.setView(this.view().y, Number(raw));
  }

  protected shiftMonth(delta: number): void {
    const { y, m } = this.view();
    const shifted = new Date(y, m - 1 + delta, 1);
    this.setView(shifted.getFullYear(), shifted.getMonth() + 1);
  }

  protected onCalendarKeydown(event: KeyboardEvent): void {
    const moves: Record<string, number> = {
      ArrowLeft: -1,
      ArrowRight: 1,
      ArrowUp: -7,
      ArrowDown: 7,
    };
    const delta = moves[event.key];
    if (delta !== undefined) {
      event.preventDefault();
      this.moveActive(delta);
      return;
    }
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      this.select(this.activeDay());
      return;
    }
    if (event.key === 'Escape') {
      event.preventDefault();
      this.close();
    }
  }

  @HostListener('document:click', ['$event'])
  protected onDocumentClick(event: MouseEvent): void {
    const target = event.target;
    if (
      this.open() &&
      target instanceof Node &&
      !(this.host.nativeElement as HTMLElement).contains(target)
    ) {
      // A click elsewhere is a dismissal, not a navigation — leave focus where it landed.
      this.close(false);
    }
  }

  private setView(y: number, m: number): void {
    this.view.set({ y, m });
    // Keep the active day inside the month on show — Jan 31 → Feb has no 31st.
    const day = Math.min(this.activeDay().d, daysInMonth(y, m));
    this.activeDay.set({ y, m, d: day });
  }

  private moveActive(delta: number): void {
    const next = addDays(this.activeDay(), delta);
    this.activeDay.set(next);
    this.view.set({ y: next.y, m: next.m });
  }
}

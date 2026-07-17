import {
  ChangeDetectionStrategy,
  Component,
  Directive,
  ElementRef,
  afterRenderEffect,
  contentChild,
  contentChildren,
  forwardRef,
  inject,
  input,
  output,
  signal,
  viewChild,
} from '@angular/core';
import { gsap } from 'gsap';
import { MOTION, prefersReducedMotion } from '../motion/motion';

/**
 * Hand-rolled menu blueprinted from the shadcn `dropdown-menu` a11y contract
 * (shadcn ships on radix/React, so only the contract is portable). Root owns
 * open state, click-outside, Esc-to-close and focus return; the trigger carries
 * `aria-haspopup`/`aria-expanded`; items are roving `role="menuitem"`s.
 */
@Component({
  selector: 'hc-dropdown-menu',
  templateUrl: './dropdown-menu.html',
  styleUrl: './dropdown-menu.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'hc-dropdown',
    '(document:click)': 'onDocumentClick($event)',
    '(document:keydown.escape)': 'onEscape()',
  },
})
export class HcDropdownMenu {
  readonly testId = input('dropdown');
  readonly ariaLabel = input<string | undefined>(undefined);

  readonly open = signal(false);

  private readonly hostEl = inject(ElementRef).nativeElement as HTMLElement;
  private readonly items = contentChildren(
    forwardRef(() => HcDropdownMenuItem),
  );
  private readonly trigger = contentChild(forwardRef(() => HcDropdownMenuTrigger));
  private readonly menuEl = viewChild<ElementRef<HTMLElement>>('menuEl');

  private readonly wantsFocusFirst = signal(false);

  constructor() {
    afterRenderEffect(() => {
      if (this.open() && this.wantsFocusFirst()) {
        this.wantsFocusFirst.set(false);
        this.items()[0]?.focus();
      }
    });

    // Fade the panel in on open; skipped under prefers-reduced-motion.
    // opacity, NOT autoAlpha: autoAlpha:0 also sets visibility:hidden, and gsap.from()
    // applies its start state immediately — hiding the menu blurs the item focused by
    // the effect above (the ArrowDown path), leaving the menu dead to the keyboard.
    afterRenderEffect(() => {
      const el = this.menuEl()?.nativeElement;
      if (this.open() && el && !prefersReducedMotion()) {
        gsap.from(el, { opacity: 0, y: -4, duration: MOTION.fast, overwrite: true });
      }
    });
  }

  toggle(): void {
    this.open.update(v => !v);
  }

  openAndFocusFirst(): void {
    this.wantsFocusFirst.set(true);
    this.open.set(true);
  }

  close(returnFocus: boolean): void {
    if (!this.open()) {
      return;
    }
    this.open.set(false);
    if (returnFocus) {
      this.trigger()?.focus();
    }
  }

  /** Move focus by `delta` from `current`, wrapping around the item list. */
  moveFocus(current: HcDropdownMenuItem, delta: number): void {
    const all = this.items();
    if (all.length === 0) {
      return;
    }
    const index = all.indexOf(current);
    const next = all[(index + delta + all.length) % all.length];
    next.focus();
  }

  protected onDocumentClick(event: MouseEvent): void {
    if (!this.open()) {
      return;
    }
    if (!this.hostEl.contains(event.target as Node)) {
      this.close(false);
    }
  }

  protected onEscape(): void {
    this.close(true);
  }
}

@Directive({
  selector: 'button[hc-dropdown-trigger]',
  host: {
    class: 'hc-dropdown__trigger',
    type: 'button',
    'aria-haspopup': 'menu',
    '[attr.aria-expanded]': 'menu.open() ? "true" : "false"',
    '(click)': 'menu.toggle()',
    '(keydown.arrowdown)': 'onArrowDown($event)',
  },
})
export class HcDropdownMenuTrigger {
  protected readonly menu = inject(HcDropdownMenu);
  private readonly host = inject(ElementRef).nativeElement as HTMLElement;

  protected onArrowDown(event: Event): void {
    event.preventDefault();
    this.menu.openAndFocusFirst();
  }

  focus(): void {
    this.host.focus();
  }
}

@Directive({
  selector: 'button[hc-dropdown-item]',
  host: {
    role: 'menuitem',
    class: 'hc-dropdown__item',
    type: 'button',
    tabindex: '-1',
    '(click)': 'activate()',
    '(keydown.arrowdown)': 'onArrow($event, 1)',
    '(keydown.arrowup)': 'onArrow($event, -1)',
  },
})
export class HcDropdownMenuItem {
  readonly select = output<void>();

  private readonly menu = inject(HcDropdownMenu);
  private readonly host = inject(ElementRef).nativeElement as HTMLElement;

  protected activate(): void {
    this.select.emit();
    this.menu.close(true);
  }

  protected onArrow(event: Event, delta: number): void {
    event.preventDefault();
    this.menu.moveFocus(this, delta);
  }

  focus(): void {
    this.host.focus();
  }
}

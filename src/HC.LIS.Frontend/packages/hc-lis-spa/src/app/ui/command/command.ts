import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  afterRenderEffect,
  computed,
  effect,
  input,
  model,
  output,
  signal,
  viewChild,
} from '@angular/core';
import { gsap } from 'gsap';
import { HcIcon } from '../icon/icon';
import { MOTION, prefersReducedMotion } from '../motion/motion';

/** A selectable command. `group` buckets it under a heading; `hint` is trailing detail. */
export interface HcCommandItem {
  id: string;
  label: string;
  group?: string;
  hint?: string;
}

interface CommandGroup {
  name: string;
  items: HcCommandItem[];
}

let nextId = 0;

/**
 * Command palette blueprinted from the shadcn `command` anatomy (dialog + input +
 * grouped list), built on the native <dialog> like hc-sheet — the browser supplies
 * the focus trap, Esc-to-close and top-layer rendering.
 *
 * Presentational by design: it renders exactly the `items` it is given and reports
 * the typed `query`, leaving filtering to the consumer. Consumers mix synchronous
 * matches with async ones (a server-side lookup can match on fields this component
 * never sees), so filtering by label here would silently drop those results.
 *
 * a11y follows the combobox-with-listbox-popup pattern: the input owns the
 * `aria-activedescendant`, and focus never leaves it while arrow keys move the
 * active option.
 */
@Component({
  selector: 'hc-command',
  imports: [HcIcon],
  templateUrl: './command.html',
  styleUrl: './command.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HcCommand {
  readonly open = model(false);
  readonly query = model('');
  readonly items = input.required<HcCommandItem[]>();
  readonly placeholder = input('Type to search…');
  readonly emptyMessage = input('No results found.');
  readonly ariaLabel = input('Command palette');
  readonly testId = input('command');

  readonly select = output<HcCommandItem>();

  private readonly uid = nextId++;
  protected readonly listId = `hc-command-list-${this.uid}`;

  protected readonly activeIndex = signal(0);

  protected readonly groups = computed<CommandGroup[]>(() => {
    const out: CommandGroup[] = [];
    for (const item of this.items()) {
      const name = item.group ?? '';
      const last = out[out.length - 1];
      if (last?.name === name) {
        last.items.push(item);
      } else {
        out.push({ name, items: [item] });
      }
    }
    return out;
  });

  protected readonly activeId = computed(() => {
    const item = this.items()[this.activeIndex()];
    return item ? this.optionId(item) : null;
  });

  private readonly dialogRef = viewChild<ElementRef<HTMLDialogElement>>('dlg');
  private readonly inputRef = viewChild<ElementRef<HTMLInputElement>>('input');

  constructor() {
    // A changed result set invalidates the old position — always highlight the top match.
    effect(() => {
      this.items();
      this.activeIndex.set(0);
    });

    afterRenderEffect(() => {
      const dialog = this.dialogRef()?.nativeElement;
      if (!dialog) {
        return;
      }
      const isOpen = dialog.hasAttribute('open');
      if (this.open() && !isOpen) {
        // Feature-detect: test DOM environments lack HTMLDialogElement methods
        if (typeof dialog.showModal === 'function') {
          dialog.showModal();
        } else {
          dialog.setAttribute('open', '');
        }
        this.inputRef()?.nativeElement.focus();
        if (!prefersReducedMotion()) {
          gsap.from(dialog, { autoAlpha: 0, y: -8, duration: MOTION.fast, overwrite: true });
        }
      } else if (!this.open() && isOpen) {
        if (typeof dialog.close === 'function') {
          dialog.close();
        } else {
          dialog.removeAttribute('open');
        }
      }
    });
  }

  protected optionId(item: HcCommandItem): string {
    return `hc-command-${this.uid}-${item.id}`;
  }

  protected isActive(item: HcCommandItem): boolean {
    return this.items()[this.activeIndex()] === item;
  }

  protected onInput(value: string): void {
    this.query.set(value);
  }

  protected onKeydown(event: KeyboardEvent): void {
    switch (event.key) {
      case 'ArrowDown':
        event.preventDefault();
        this.move(1);
        break;
      case 'ArrowUp':
        event.preventDefault();
        this.move(-1);
        break;
      case 'Enter': {
        event.preventDefault();
        const item = this.items()[this.activeIndex()];
        if (item) {
          this.choose(item);
        }
        break;
      }
      default:
        break;
    }
  }

  protected choose(item: HcCommandItem): void {
    this.select.emit(item);
    this.open.set(false);
  }

  protected onNativeClose(): void {
    this.open.set(false);
  }

  private move(delta: number): void {
    const count = this.items().length;
    if (count === 0) {
      return;
    }
    this.activeIndex.update(i => (i + delta + count) % count);
  }
}

import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
  output,
  signal,
} from '@angular/core';

export interface HcComboboxOption {
  value: string;
  label: string;
}

let comboboxSeq = 0;

/**
 * Typeahead combobox: the consumer performs the search (listen to queryChange,
 * feed results back through options) and reacts to selection.
 */
@Component({
  selector: 'hc-combobox',
  templateUrl: './combobox.html',
  styleUrl: './combobox.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HcCombobox {
  readonly options = input<readonly HcComboboxOption[]>([]);
  readonly placeholder = input('');

  readonly queryChange = output<string>();
  readonly selected = output<HcComboboxOption>();

  protected readonly listboxId = `hc-combobox-${++comboboxSeq}`;
  protected readonly query = signal('');
  protected readonly dismissed = signal(false);
  protected readonly activeIndex = signal(-1);

  protected readonly expanded = computed(
    () => !this.dismissed() && this.query().length > 0 && this.options().length > 0,
  );

  protected readonly activeId = computed(() => {
    const index = this.activeIndex();
    return this.expanded() && index >= 0 ? this.optionId(index) : null;
  });

  protected optionId(index: number): string {
    return `${this.listboxId}-option-${index}`;
  }

  protected onInput(value: string): void {
    this.query.set(value);
    this.dismissed.set(false);
    this.activeIndex.set(-1);
    this.queryChange.emit(value);
  }

  protected select(option: HcComboboxOption): void {
    this.query.set(option.label);
    this.dismissed.set(true);
    this.activeIndex.set(-1);
    this.selected.emit(option);
  }

  protected onKeydown(event: KeyboardEvent): void {
    if (!this.expanded()) {
      return;
    }
    switch (event.key) {
      case 'ArrowDown':
        event.preventDefault();
        this.activeIndex.update(i => Math.min(i + 1, this.options().length - 1));
        break;
      case 'ArrowUp':
        event.preventDefault();
        this.activeIndex.update(i => Math.max(i - 1, 0));
        break;
      case 'Enter': {
        const option = this.options()[this.activeIndex()];
        if (option) {
          event.preventDefault();
          this.select(option);
        }
        break;
      }
      case 'Escape':
        event.preventDefault();
        this.dismissed.set(true);
        break;
      default:
        break;
    }
  }
}

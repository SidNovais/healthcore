import {
  ChangeDetectionStrategy,
  Component,
  Directive,
  ElementRef,
  computed,
  contentChildren,
  forwardRef,
  inject,
  input,
  model,
} from '@angular/core';

@Component({
  selector: 'hc-tabs',
  templateUrl: './tabs.html',
  styleUrl: './tabs.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HcTabs {
  readonly value = model.required<string>();

  private readonly tabs = contentChildren(
    forwardRef(() => HcTab),
    { descendants: true },
  );

  /** Roving selection: move from `current` by `delta`, wrapping around. */
  move(current: string, delta: number): void {
    const all = this.tabs();
    if (all.length === 0) {
      return;
    }
    const index = all.findIndex(tab => tab.value() === current);
    const next = all[(index + delta + all.length) % all.length];
    this.value.set(next.value());
    next.focus();
  }
}

@Directive({
  selector: 'button[hc-tab]',
  host: {
    role: 'tab',
    class: 'hc-tab',
    type: 'button',
    '[attr.aria-selected]': 'selected() ? "true" : "false"',
    '[attr.tabindex]': 'selected() ? 0 : -1',
    '(click)': 'activate()',
    '(keydown.arrowright)': 'tabs.move(value(), 1)',
    '(keydown.arrowleft)': 'tabs.move(value(), -1)',
  },
})
export class HcTab {
  readonly value = input.required<string>();

  protected readonly tabs = inject(HcTabs);
  private readonly host = inject(ElementRef).nativeElement as HTMLElement;

  protected readonly selected = computed(() => this.tabs.value() === this.value());

  protected activate(): void {
    this.tabs.value.set(this.value());
  }

  focus(): void {
    this.host.focus();
  }
}

@Component({
  selector: 'hc-tab-panel',
  template: '<ng-content />',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    role: 'tabpanel',
    class: 'hc-tab-panel',
    '[attr.hidden]': 'active() ? null : ""',
  },
})
export class HcTabPanel {
  readonly value = input.required<string>();

  private readonly tabs = inject(HcTabs);

  protected readonly active = computed(() => this.tabs.value() === this.value());
}

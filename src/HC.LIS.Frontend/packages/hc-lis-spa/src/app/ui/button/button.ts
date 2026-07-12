import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

export type HcButtonVariant = 'default' | 'cta' | 'ghost' | 'destructive';
export type HcButtonSize = 'sm' | 'md' | 'icon';

@Component({
  selector: 'button[hc-button], a[hc-button]',
  templateUrl: './button.html',
  styleUrl: './button.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class]': 'classes()',
    '[attr.aria-busy]': 'loading() ? "true" : null',
    '[attr.disabled]': 'loading() ? "" : null',
  },
})
export class HcButton {
  readonly variant = input<HcButtonVariant>('default');
  readonly size = input<HcButtonSize>('md');
  readonly loading = input(false);

  protected readonly classes = computed(
    () => `hc-btn hc-btn--${this.variant()} hc-btn--${this.size()}`,
  );
}

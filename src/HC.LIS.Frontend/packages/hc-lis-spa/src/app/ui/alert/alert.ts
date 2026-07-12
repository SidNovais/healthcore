import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

export type HcAlertVariant = 'info' | 'success' | 'warning' | 'error';

@Component({
  selector: 'hc-alert',
  templateUrl: './alert.html',
  styleUrl: './alert.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    role: 'alert',
    '[class]': 'classes()',
  },
})
export class HcAlert {
  readonly variant = input<HcAlertVariant>('info');
  readonly title = input<string | null>(null);

  protected readonly classes = computed(() => `hc-alert hc-alert--${this.variant()}`);
}

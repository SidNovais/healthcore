import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

export type HcBadgeVariant = 'neutral' | 'accent' | 'success' | 'warning' | 'error';

@Component({
  selector: 'hc-badge',
  template: '<ng-content />',
  styleUrl: './badge.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class]': 'classes()',
    '[attr.data-role]': 'role()',
  },
})
export class HcBadge {
  readonly variant = input<HcBadgeVariant>('neutral');
  /** User role (e.g. 'LabTechnician') — colors from the --color-role-* tokens. */
  readonly role = input<string | null>(null);

  protected readonly classes = computed(() =>
    this.role() ? 'hc-badge hc-badge--role' : `hc-badge hc-badge--${this.variant()}`,
  );
}

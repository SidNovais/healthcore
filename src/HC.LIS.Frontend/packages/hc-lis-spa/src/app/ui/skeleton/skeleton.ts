import { ChangeDetectionStrategy, Component } from '@angular/core';

/**
 * Standard number of placeholder rows to render while a list/table view loads.
 * Iterate it with `@for` to keep skeleton row counts uniform across features.
 */
export const SKELETON_ROWS: readonly number[] = Array.from({ length: 5 }, (_, i) => i);

/** Decorative loading placeholder; size it from the consumer's CSS. */
@Component({
  selector: 'hc-skeleton',
  template: '',
  styleUrl: './skeleton.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'hc-skeleton',
    'aria-hidden': 'true',
  },
})
export class HcSkeleton {}

import { ChangeDetectionStrategy, Component } from '@angular/core';

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

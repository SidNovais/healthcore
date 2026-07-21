import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

/**
 * Determinate/indeterminate progress bar for heavy jobs — bulk-processing many
 * rows, uploading a large file. Prefer `hc-spinner` for short indeterminate
 * waits and `hc-skeleton` while a view's data is first rendering.
 */
@Component({
  selector: 'hc-progress',
  template: `
    <span class="hc-progress__track" aria-hidden="true">
      <span
        class="hc-progress__fill"
        [style.width.%]="indeterminate() ? null : clampedValue()"
      ></span>
    </span>
  `,
  styleUrl: './progress.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'hc-progress',
    role: 'progressbar',
    'aria-valuemin': '0',
    'aria-valuemax': '100',
    '[class.hc-progress--indeterminate]': 'indeterminate()',
    '[attr.aria-valuenow]': 'indeterminate() ? null : clampedValue()',
    '[attr.aria-label]': 'label() || null',
    '[attr.data-testid]': 'testId() || null',
  },
})
export class HcProgress {
  /** Completion percentage, 0–100. Ignored when `indeterminate` is set. */
  readonly value = input(0);
  /** When true, shows an ongoing job with no measurable progress. */
  readonly indeterminate = input(false);
  readonly label = input('');
  readonly testId = input('');

  protected readonly clampedValue = computed(() =>
    Math.round(Math.min(100, Math.max(0, this.value()))),
  );
}

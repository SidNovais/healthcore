import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/**
 * Loading spinner. It fades in only after 300ms (CSS delay) so fast responses
 * never flash a loader.
 */
@Component({
  selector: 'hc-spinner',
  template: '<span class="hc-spinner__circle" aria-hidden="true"></span>',
  styleUrl: './spinner.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'hc-spinner',
    role: 'status',
    '[attr.aria-label]': 'label()',
  },
})
export class HcSpinner {
  readonly label = input('Loading');
}

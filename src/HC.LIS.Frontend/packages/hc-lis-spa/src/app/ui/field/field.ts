import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/** Groups a label + control with helper/error text below it. */
@Component({
  selector: 'hc-field',
  templateUrl: './field.html',
  styleUrl: './field.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HcField {
  readonly helper = input<string | null>(null);
  readonly error = input<string | null>(null);
}

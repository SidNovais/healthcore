import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/** Empty state: title + optional description + projected icon/actions. */
@Component({
  selector: 'hc-empty',
  templateUrl: './empty.html',
  styleUrl: './empty.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HcEmpty {
  readonly title = input.required<string>();
  readonly description = input<string | null>(null);
}

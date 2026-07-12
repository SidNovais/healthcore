import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

/** Design-system table: sticky header, tabular numerals, optional dense rows. */
@Component({
  selector: 'table[hc-table]',
  template: '<ng-content />',
  styleUrl: './table.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { '[class]': 'classes()' },
})
export class HcTable {
  readonly dense = input(false);

  protected readonly classes = computed(
    () => `hc-table tabular-nums${this.dense() ? ' hc-table--dense' : ''}`,
  );
}

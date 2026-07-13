import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { HcIcon } from '../icon/icon';

/** A page slot in the rendered control: a concrete page number or a truncation gap. */
export type HcPageSlot = number | 'ellipsis';

/**
 * Page navigator for the data tables. 1-based `page` / `pageCount`; emits
 * `pageChange` with the requested page. Blueprinted from the shadcn `pagination`
 * a11y contract: a `nav[aria-label]` landmark, the active page carries
 * `aria-current="page"`, and truncation ellipses are `aria-hidden`.
 */
@Component({
  selector: 'hc-pagination',
  imports: [HcIcon],
  templateUrl: './pagination.html',
  styleUrl: './pagination.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    role: 'navigation',
    class: 'hc-pagination',
    '[attr.aria-label]': 'ariaLabel()',
    '[attr.data-testid]': 'testId()',
  },
})
export class HcPagination {
  readonly page = input.required<number>();
  readonly pageCount = input.required<number>();
  /** Base for the child `data-testid`s (`{testId}-prev`, `-next`, `-page-{n}`). */
  readonly testId = input('pagination');
  readonly ariaLabel = input('pagination');

  readonly pageChange = output<number>();

  protected readonly slots = computed<HcPageSlot[]>(() => {
    const total = this.pageCount();
    const current = this.page();
    if (total <= 7) {
      return Array.from({ length: total }, (_, i) => i + 1);
    }
    const shown = new Set(
      [1, total, current - 1, current, current + 1].filter(n => n >= 1 && n <= total),
    );
    const sorted = [...shown].sort((a, b) => a - b);
    const out: HcPageSlot[] = [];
    let prev = 0;
    for (const n of sorted) {
      if (n - prev > 1) {
        out.push('ellipsis');
      }
      out.push(n);
      prev = n;
    }
    return out;
  });

  protected readonly onFirst = computed(() => this.page() <= 1);
  protected readonly onLast = computed(() => this.page() >= this.pageCount());

  protected isCurrent(slot: HcPageSlot): boolean {
    return slot === this.page();
  }

  protected go(target: number): void {
    if (target < 1 || target > this.pageCount() || target === this.page()) {
      return;
    }
    this.pageChange.emit(target);
  }
}

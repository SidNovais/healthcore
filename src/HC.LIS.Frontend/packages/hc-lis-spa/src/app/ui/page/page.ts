import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { HcBreadcrumb, type HcBreadcrumbItem } from '../breadcrumb/breadcrumb';

/** How wide the page's content may grow. Every measure is centred. */
export type HcPageWidth = 'wide' | 'narrow' | 'full';

/**
 * The frame every routed page sits in: one centred container, an optional breadcrumb
 * trail, the title block, and a slot for the page's actions.
 *
 * Pages declare *what* they are — a wide table, a narrow form — and never *how* to
 * centre. Before this existed all 12 pages hand-rolled their own root, producing seven
 * different max-widths, three different places for the header actions, and a mix of h1
 * and h2 titles.
 *
 * The title is an **input, not a slot**: the heading level is not the page's choice, and
 * a slot would let the h1/h2 drift straight back in. The actions stay a projected slot
 * because their content is arbitrary. This mirrors hc-breadcrumb's data-driven `items`.
 *
 * Not for surfaces outside the app shell: login is a full-viewport auth surface centred
 * on both axes, and the error pages are their own thing. Neither is a page-with-a-header.
 */
@Component({
  selector: 'hc-page',
  imports: [HcBreadcrumb],
  templateUrl: './page.html',
  styleUrl: './page.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HcPage {
  readonly title = input.required<string>();
  readonly subtitle = input<string | undefined>(undefined);
  /** Ancestor trail for deep pages. Omit on top-level pages — there is nothing to trail. */
  readonly breadcrumbs = input<HcBreadcrumbItem[] | undefined>(undefined);
  readonly width = input<HcPageWidth>('wide');
  /** Rendered on the page container; also the prefix the child testids derive from. */
  readonly testId = input<string | undefined>(undefined);
  /**
   * Overrides for pages whose ids predate this primitive and do not follow the derived
   * pattern — register-patient's heading is `register-patient-heading`, and
   * order-detail's breadcrumb is `order-breadcrumb` while its container is
   * `order-detail`. Those ids are the e2e regression net, so pages override here rather
   * than get renamed.
   */
  readonly titleTestId = input<string | undefined>(undefined);
  readonly breadcrumbTestId = input<string | undefined>(undefined);

  protected resolvedBreadcrumbTestId(): string {
    const override = this.breadcrumbTestId();
    if (override) {
      return override;
    }
    const id = this.testId();
    return id ? `${id}-breadcrumb` : 'breadcrumb';
  }

  /**
   * Pages that already had a title testid keep it here — `worklist-title`,
   * `users-title` and `triage-title` all fall out of the derivation; only
   * `register-patient-heading` needs the override.
   */
  protected resolvedTitleTestId(): string | null {
    const override = this.titleTestId();
    if (override) {
      return override;
    }
    const id = this.testId();
    return id ? `${id}-title` : null;
  }
}

import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HcIcon } from '../icon/icon';

/** A crumb in the trail. The final crumb is the current page and is never linked. */
export interface HcBreadcrumbItem {
  label: string;
  route?: string;
}

/**
 * Trail of ancestor routes for deep pages. Blueprinted from the shadcn `breadcrumb`
 * a11y contract: a `nav[aria-label="Breadcrumb"]` landmark wrapping an ordered list,
 * the current page exposed via `aria-current="page"` rather than a link, and purely
 * decorative separators hidden from assistive tech.
 *
 * Data-driven (`items`) rather than composed from sub-components — the trails here are
 * short and static, and this keeps consumers to a single element, as hc-pagination does.
 */
@Component({
  selector: 'hc-breadcrumb',
  imports: [RouterLink, HcIcon],
  templateUrl: './breadcrumb.html',
  styleUrl: './breadcrumb.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    role: 'navigation',
    class: 'hc-breadcrumb',
    '[attr.aria-label]': 'ariaLabel()',
    '[attr.data-testid]': 'testId()',
  },
})
export class HcBreadcrumb {
  readonly items = input.required<HcBreadcrumbItem[]>();
  /** Base for the child `data-testid`s (`{testId}-link-{index}`, `{testId}-page`). */
  readonly testId = input('breadcrumb');
  readonly ariaLabel = input('Breadcrumb');
}

import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { gsap } from 'gsap';
import { HcButton } from '../../ui/button/button';
import { HcCard } from '../../ui/card/card';
import { HcIcon, type HcIconName } from '../../ui/icon/icon';
import { MOTION, useMotion } from '../../ui/motion/motion';

/**
 * The shared body of the error routes. Deliberately not hc-page: these are
 * full-bleed dead ends centred on both axes, not pages with a header and actions.
 *
 * unauthorized and not-found were near-verbatim copies of each other — the same
 * .centered-page/.error-card/.error-code CSS, the same entrance tween, and the same
 * component boilerplate, differing only in what is now an input.
 */
@Component({
  selector: 'app-error-page',
  standalone: true,
  imports: [RouterLink, HcButton, HcCard, HcIcon],
  templateUrl: './error-page.component.html',
  styleUrl: './error-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ErrorPageComponent {
  /** The status code. Rendered as the page's h1 — it is what names the page. */
  readonly code = input.required<string>();
  readonly heading = input.required<string>();
  readonly message = input.required<string>();
  /** Decorative only; the heading already carries the meaning. */
  readonly icon = input<HcIconName | undefined>(undefined);
  /**
   * `error` for a refusal the user ran into (403), `muted` for a dead end that is
   * nobody's fault (404).
   */
  readonly tone = input<'error' | 'muted'>('muted');
  /**
   * Structural: `main` for routes inside the app shell, which fill the outlet;
   * `viewport` for routes outside it, which own the whole screen.
   */
  readonly fill = input<'main' | 'viewport'>('main');
  readonly testId = input<string | undefined>(undefined);

  constructor() {
    // autoAlpha is safe here — this card takes no focus, so hiding it at the tween's
    // start state cannot blur anything (unlike the palette/menu/picker entrances).
    useMotion(ctx => {
      if (ctx.reduceMotion) {
        return;
      }
      gsap.from('.error-card', { autoAlpha: 0, y: 10, duration: MOTION.slow });
    });
  }
}

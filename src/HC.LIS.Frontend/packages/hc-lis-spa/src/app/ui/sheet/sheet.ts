import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  afterRenderEffect,
  input,
  model,
  viewChild,
} from '@angular/core';
import { gsap } from 'gsap';
import { MOTION, prefersReducedMotion } from '../motion/motion';

/**
 * Right-anchored slide-over built on the native <dialog> element — the browser
 * supplies the same focus trap, Esc-to-close and top-layer rendering as
 * hc-dialog, but the panel is pinned to the inline-end edge and slides in from
 * the right (skipped under prefers-reduced-motion). Blueprinted from the shadcn
 * `sheet` anatomy.
 */
@Component({
  selector: 'hc-sheet',
  templateUrl: './sheet.html',
  styleUrl: './sheet.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HcSheet {
  readonly open = model(false);
  readonly ariaLabel = input<string | undefined>(undefined);
  /**
   * Rendered on the <dialog> panel, not the host. The host box is 0x0 — it is
   * display:inline and its only child is position:fixed — so a testid there is
   * never "visible" to a browser-driven test even when the sheet is on screen.
   */
  readonly testId = input<string | undefined>(undefined);

  private readonly dialogRef = viewChild.required<ElementRef<HTMLDialogElement>>('dlg');

  constructor() {
    afterRenderEffect(() => {
      const dialog = this.dialogRef().nativeElement;
      const isOpen = dialog.hasAttribute('open');
      if (this.open() && !isOpen) {
        // Feature-detect: test DOM environments lack HTMLDialogElement methods
        if (typeof dialog.showModal === 'function') {
          dialog.showModal();
        } else {
          dialog.setAttribute('open', '');
        }
        if (!prefersReducedMotion()) {
          gsap.from(dialog, { xPercent: 100, duration: MOTION.normal, overwrite: true });
        }
      } else if (!this.open() && isOpen) {
        if (typeof dialog.close === 'function') {
          dialog.close();
        } else {
          dialog.removeAttribute('open');
        }
      }
    });
  }

  protected onNativeClose(): void {
    this.open.set(false);
  }
}

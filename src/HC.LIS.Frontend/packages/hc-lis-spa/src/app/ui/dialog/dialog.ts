import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  afterRenderEffect,
  input,
  model,
  viewChild,
} from '@angular/core';

/**
 * How wide the dialog panel is. `narrow` shrink-wraps the content under a cap, as the
 * native <dialog> does; `wide` sets an explicit forms measure, because a column of
 * full-width inputs has too little intrinsic width to size a panel on its own.
 */
export type HcDialogWidth = 'narrow' | 'wide';

/**
 * Modal dialog on the native <dialog> element — the browser supplies the focus
 * trap, Esc-to-close and top-layer rendering; the scrim is styled via ::backdrop.
 * A click on that scrim light-dismisses the dialog (see onBackdropClick).
 */
@Component({
  selector: 'hc-dialog',
  templateUrl: './dialog.html',
  styleUrl: './dialog.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HcDialog {
  readonly open = model(false);
  /**
   * Rendered on the <dialog> panel, not the host. The host box is 0x0 — it is
   * display:inline and its only child is position:fixed — so a testid there is
   * never "visible" to a browser-driven test even when the dialog is on screen.
   */
  readonly testId = input<string | undefined>(undefined);
  /**
   * Dialogs hosting a multi-field form declare `wide`; single-control dialogs (one
   * select, one textarea, a confirm) stay narrow. Content must not re-cap itself on top
   * of this — that is what left create-user's form at 420px inside a 32rem panel.
   */
  readonly width = input<HcDialogWidth>('narrow');

  private readonly dialogRef = viewChild.required<ElementRef<HTMLDialogElement>>('dlg');

  /**
   * Whether the current press began on the backdrop. Tracked so that selecting text
   * inside a field and releasing past the panel edge does not dismiss — only a press
   * that both starts and ends on the backdrop counts as a backdrop click.
   */
  private _pressStartedOnBackdrop = false;

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

  /**
   * The <dialog> is the event target only for clicks on the backdrop or the panel's own
   * border — projected content sits in an inner wrapper that swallows those clicks. A
   * press that both starts and ends there is a backdrop dismiss.
   */
  protected onBackdropMouseDown(event: MouseEvent): void {
    this._pressStartedOnBackdrop = event.target === this.dialogRef().nativeElement;
  }

  protected onBackdropClick(event: MouseEvent): void {
    if (event.target === this.dialogRef().nativeElement && this._pressStartedOnBackdrop) {
      this.open.set(false);
    }
  }
}

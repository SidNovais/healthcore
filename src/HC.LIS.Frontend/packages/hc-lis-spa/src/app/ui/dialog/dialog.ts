import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  afterRenderEffect,
  model,
  viewChild,
} from '@angular/core';

/**
 * Modal dialog on the native <dialog> element — the browser supplies the focus
 * trap, Esc-to-close and top-layer rendering; the scrim is styled via ::backdrop.
 */
@Component({
  selector: 'hc-dialog',
  templateUrl: './dialog.html',
  styleUrl: './dialog.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HcDialog {
  readonly open = model(false);

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

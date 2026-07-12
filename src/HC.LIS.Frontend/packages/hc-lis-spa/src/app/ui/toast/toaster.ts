import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ToastService } from './toast.service';

/** Renders active toasts in a polite live region; mount once in the shell. */
@Component({
  selector: 'hc-toaster',
  templateUrl: './toaster.html',
  styleUrl: './toaster.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HcToaster {
  protected readonly toastService = inject(ToastService);
}

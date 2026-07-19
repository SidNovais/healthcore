import { Component, input, output, signal, inject, OnInit } from '@angular/core';
import { PrintLabelsCardComponent } from './print-labels-card.component';
import { TriageService } from './triage.service';
import type { SampleSummary } from '../../core/domain/sample-summary';
import { HcButton } from '../../ui/button/button';
import { HcIcon } from '../../ui/icon/icon';
import { HcDialog } from '../../ui/dialog/dialog';
import { HcSkeleton } from '../../ui/skeleton/skeleton';

@Component({
  selector: 'app-print-labels-modal',
  standalone: true,
  imports: [PrintLabelsCardComponent, HcButton, HcIcon, HcDialog, HcSkeleton],
  templateUrl: './print-labels-modal.component.html',
  styleUrl: './print-labels-modal.component.css',
})
export class PrintLabelsModalComponent implements OnInit {
  private readonly triageService = inject(TriageService);

  readonly collectionRequestId = input.required<string>();
  /** Resolved patient name for the modal header; null falls back to a placeholder. */
  readonly patientName = input<string | null>(null);
  readonly closed = output<void>();

  protected readonly samples = signal<SampleSummary[]>([]);
  protected readonly loading = signal(true);
  /** Opens the native <dialog> immediately; flips false on Esc/backdrop so hc-dialog does not re-open it. */
  protected readonly dialogOpen = signal(true);

  async ngOnInit(): Promise<void> {
    try {
      const result = await this.triageService.getSamples(this.collectionRequestId());
      this.samples.set(result);
    } finally {
      this.loading.set(false);
    }
  }

  protected close(): void {
    this.dialogOpen.set(false);
    this.closed.emit();
  }

  protected onDialogOpenChange(open: boolean): void {
    if (!open) {
      this.close();
    }
  }

  protected printLabels(): void {
    document.body.classList.add('printing-labels');
    window.addEventListener('afterprint', () => {
      document.body.classList.remove('printing-labels');
    }, { once: true });
    window.print();
  }
}

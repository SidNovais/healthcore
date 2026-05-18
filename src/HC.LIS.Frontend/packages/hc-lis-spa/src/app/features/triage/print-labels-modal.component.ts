import { Component, input, output, signal, inject, OnInit } from '@angular/core';
import { PrintLabelsCardComponent } from './print-labels-card.component';
import { TriageService } from './triage.service';
import type { SampleSummary } from '../../core/domain/sample-summary';

@Component({
  selector: 'app-print-labels-modal',
  standalone: true,
  imports: [PrintLabelsCardComponent],
  template: `
    <div
      data-testid="print-labels-modal"
      class="overlay"
      (click)="onOverlayClick($event)"
    >
      <div class="modal-box">
        <div class="modal-header">
          <div>
            <h2 class="modal-title">Print Labels</h2>
            <p class="modal-sub">{{ patientId() }}</p>
          </div>
        </div>

        @if (loading()) {
          <p class="loading-text">Loading samples…</p>
        } @else {
          <app-print-labels-card [samples]="samples()" [showPrintButton]="false" />
        }

        <div class="modal-footer">
          <button data-testid="print-modal-cancel-btn" class="btn-ghost" (click)="closed.emit()">Cancel</button>
          @if (!loading() && samples().length > 0) {
            <button data-testid="print-modal-print-btn" class="btn-print" (click)="printLabels()">🖨 Print</button>
          }
        </div>
      </div>
    </div>
  `,
  styles: [`
    .overlay {
      position: fixed; inset: 0;
      background: rgba(0,0,0,0.45);
      display: flex; align-items: center; justify-content: center;
      z-index: 50;
    }
    .modal-box {
      background: #fff;
      border-radius: 10px;
      padding: 24px;
      width: 420px;
      max-width: calc(100vw - 2rem);
      box-shadow: 0 20px 60px rgba(0,0,0,0.2);
    }
    .modal-title { font-size: 1rem; font-weight: 700; margin-bottom: 2px; }
    .modal-sub { font-size: 0.75rem; color: #6b7280; margin-bottom: 1rem; }
    .loading-text { color: #9ca3af; font-size: 0.85rem; margin: 1rem 0; }
    .modal-footer { display: flex; justify-content: flex-end; gap: 8px; margin-top: 1rem; padding-top: 0.75rem; border-top: 1px solid #f3f4f6; }
    .btn-ghost { padding: 6px 14px; border-radius: 4px; font-size: 13px; font-weight: 500; border: 1px solid #d1d5db; background: #f9fafb; cursor: pointer; color: #374151; }
    .btn-print { padding: 6px 14px; border-radius: 4px; font-size: 13px; font-weight: 500; border: none; background: #4f46e5; color: #fff; cursor: pointer; }
  `],
})
export class PrintLabelsModalComponent implements OnInit {
  private readonly triageService = inject(TriageService);

  readonly collectionRequestId = input.required<string>();
  readonly patientId = input.required<string>();
  readonly closed = output<void>();

  protected readonly samples = signal<SampleSummary[]>([]);
  protected readonly loading = signal(true);

  async ngOnInit(): Promise<void> {
    const result = await this.triageService.getSamples(this.collectionRequestId());
    this.samples.set(result);
    this.loading.set(false);
  }

  protected onOverlayClick(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('overlay')) {
      this.closed.emit();
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

import { Component, input, output } from '@angular/core';
import { SlicePipe } from '@angular/common';
import type { CollectionRequestSummary } from '../../core/domain/collection-request-summary';

@Component({
  selector: 'app-arrived-patient-card',
  standalone: true,
  imports: [SlicePipe],
  template: `
    <div data-testid="arrived-patient-card" class="card">
      <div class="card-row">
        <div class="card-info">
          <span class="label">Patient ID</span>
          <span class="value mono">{{ item().patientId | slice:0:8 }}…</span>
        </div>
        <div class="card-info">
          <span class="label">Arrived</span>
          <span class="value">{{ item().arrivedAt | slice:0:10 }}</span>
        </div>
        <span class="status-badge status-arrived">Arrived</span>
        <div class="actions">
          <button
            data-testid="send-to-waiting-btn"
            class="btn-action btn-send"
            (click)="onSend()"
          >
            Send to Waiting Room
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .card { border: 1px solid #e5e7eb; border-radius: 2px; background: #fff; padding: 0.75rem 1rem; }
    .card-row { display: flex; align-items: center; gap: 1.5rem; flex-wrap: wrap; }
    .card-info { display: flex; flex-direction: column; gap: 0.1rem; }
    .label { font-size: 0.65rem; text-transform: uppercase; letter-spacing: 0.08em; color: #9ca3af; }
    .value { font-size: 0.875rem; color: #111; }
    .mono { font-family: 'JetBrains Mono', 'IBM Plex Mono', monospace; font-size: 0.8rem; }
    .status-badge { padding: 0.2rem 0.5rem; font-size: 0.7rem; font-weight: 600; text-transform: uppercase; letter-spacing: 0.06em; border-radius: 2px; }
    .status-arrived { background: #d1fae5; color: #065f46; }
    .actions { margin-left: auto; }
    .btn-action { padding: 0.3rem 0.75rem; border: none; border-radius: 2px; font-size: 0.8rem; cursor: pointer; font-weight: 500; }
    .btn-send { background: #00897B; color: #fff; }
  `],
})
export class ArrivedPatientCardComponent {
  readonly item = input.required<CollectionRequestSummary>();
  readonly sendToWaiting = output<string>();

  protected onSend(): void {
    this.sendToWaiting.emit(this.item().collectionRequestId);
  }
}

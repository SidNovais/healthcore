import { Component, input, signal, OnInit, inject } from '@angular/core';
import { SlicePipe } from '@angular/common';
import { PrintLabelsCardComponent } from './print-labels-card.component';
import { TriageService } from './triage.service';
import type { CollectionRequestSummary } from '../../core/domain/collection-request-summary';
import type { SampleSummary } from '../../core/domain/sample-summary';

@Component({
  selector: 'app-preparing-patient-card',
  standalone: true,
  imports: [SlicePipe, PrintLabelsCardComponent],
  template: `
    <div data-testid="preparing-patient-card" class="card">
      <div class="card-row">
        <div class="card-info">
          <span class="label">Patient ID</span>
          <span class="value mono">{{ item().patientId | slice:0:8 }}…</span>
        </div>
        <div class="card-info">
          <span class="label">Arrived</span>
          <span class="value">{{ item().arrivedAt | slice:0:10 }}</span>
        </div>
        <span class="status-badge status-waiting">Waiting</span>
      </div>

      <div class="samples-section">
        @if (loading()) {
          <p class="no-samples">Loading samples…</p>
        } @else if (samples().length === 0) {
          <p class="no-samples">No samples found.</p>
        } @else {
          <app-print-labels-card [samples]="samples()" />
        }
      </div>
    </div>
  `,
  styles: [`
    .card { border: 1px solid #e5e7eb; border-radius: 2px; background: #fff; padding: 0.75rem 1rem; display: flex; flex-direction: column; gap: 0.75rem; }
    .card-row { display: flex; align-items: center; gap: 1.5rem; flex-wrap: wrap; }
    .card-info { display: flex; flex-direction: column; gap: 0.1rem; }
    .label { font-size: 0.65rem; text-transform: uppercase; letter-spacing: 0.08em; color: #9ca3af; }
    .value { font-size: 0.875rem; color: #111; }
    .mono { font-family: 'JetBrains Mono', 'IBM Plex Mono', monospace; font-size: 0.8rem; }
    .status-badge { padding: 0.2rem 0.5rem; font-size: 0.7rem; font-weight: 600; text-transform: uppercase; letter-spacing: 0.06em; border-radius: 2px; }
    .status-waiting { background: #fef3c7; color: #92400e; }
    .samples-section { border-top: 1px solid #f3f4f6; padding-top: 0.5rem; display: flex; flex-direction: column; gap: 0.4rem; }
    .no-samples { color: #9ca3af; font-size: 0.8rem; margin: 0; }
  `],
})
export class PreparingPatientCardComponent implements OnInit {
  private readonly triageService = inject(TriageService);

  readonly item = input.required<CollectionRequestSummary>();

  protected samples = signal<SampleSummary[]>([]);
  protected readonly loading = signal(true);

  async ngOnInit(): Promise<void> {
    const result = await this.triageService.getSamples(this.item().collectionRequestId);
    this.samples.set(result);
    this.loading.set(false);
  }
}

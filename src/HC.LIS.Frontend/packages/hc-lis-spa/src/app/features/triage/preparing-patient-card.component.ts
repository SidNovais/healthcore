import { Component, input, output, signal, OnInit, inject } from '@angular/core';
import { SlicePipe } from '@angular/common';
import { BarcodeFormComponent } from './barcode-form.component';
import { TriageService } from './triage.service';
import { AuthService } from '../../core/application/auth.service';
import type { CollectionRequestSummary } from '../../core/domain/collection-request-summary';
import type { SampleSummary } from '../../core/domain/sample-summary';

@Component({
  selector: 'app-preparing-patient-card',
  standalone: true,
  imports: [SlicePipe, BarcodeFormComponent],
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
        @for (sample of samples(); track sample.id) {
          <div class="sample-row">
            @if (sample.barcode) {
              <div class="sample-done">
                <span class="tube-label mono">{{ sample.tubeType }}</span>
                <span class="barcode-value mono">{{ sample.barcode }}</span>
                <span class="badge-done">Barcode Created</span>
              </div>
            } @else {
              <app-barcode-form
                [tubeType]="sample.tubeType"
                (barcodeCreated)="onBarcodeCreated(sample, $event)"
              />
            }
          </div>
        }
        @if (loading()) {
          <p class="no-samples">Loading samples…</p>
        } @else if (samples().length === 0) {
          <p class="no-samples">No samples found.</p>
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
    .sample-row { }
    .sample-done { display: flex; align-items: center; gap: 0.75rem; padding: 0.35rem 0; }
    .tube-label { font-size: 0.8rem; font-weight: 600; color: #374151; min-width: 60px; }
    .barcode-value { color: #6b7280; font-size: 0.75rem; }
    .badge-done { font-size: 0.65rem; padding: 0.1rem 0.4rem; background: #d1fae5; color: #065f46; border-radius: 2px; font-weight: 600; text-transform: uppercase; }
    .no-samples { color: #9ca3af; font-size: 0.8rem; margin: 0; }
  `],
})
export class PreparingPatientCardComponent implements OnInit {
  private readonly triageService = inject(TriageService);
  private readonly auth = inject(AuthService);

  readonly item = input.required<CollectionRequestSummary>();
  readonly barcodeCreated = output<void>();

  protected samples = signal<SampleSummary[]>([]);
  protected readonly loading = signal(true);

  async ngOnInit(): Promise<void> {
    const result = await this.triageService.getSamples(this.item().collectionRequestId);
    this.samples.set(result);
    this.loading.set(false);
  }

  protected async onBarcodeCreated(sample: SampleSummary, barcodeValue: string): Promise<void> {
    const technicianId = this.auth.currentUser()?.userId;
    if (!technicianId) {
      console.error('Cannot create barcode: no authenticated user');
      return;
    }
    await this.triageService.createBarcode(this.item().collectionRequestId, sample.tubeType, barcodeValue, technicianId);
    const updated = await this.triageService.getSamples(this.item().collectionRequestId);
    this.samples.set(updated);
    this.barcodeCreated.emit();
  }
}

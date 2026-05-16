import { Component, inject, OnInit, signal } from '@angular/core';
import { TriageService } from './triage.service';
import { ArrivedPatientCardComponent } from './arrived-patient-card.component';
import { PreparingPatientCardComponent } from './preparing-patient-card.component';

@Component({
  selector: 'app-triage',
  standalone: true,
  imports: [ArrivedPatientCardComponent, PreparingPatientCardComponent],
  template: `
    <div class="page">
      <div class="page-header">
        <h1 data-testid="triage-title" class="page-title">Triage</h1>
        <button class="btn-refresh" (click)="refresh()">Refresh</button>
      </div>

      <section class="section" data-testid="arriving-section">
        <h2 class="section-title">Arriving</h2>
        @if (service.arrived().length === 0) {
          <div data-testid="arriving-empty-state" class="empty-state">
            <p>No patients arriving.</p>
          </div>
        } @else {
          <div class="card-list">
            @for (item of service.arrived(); track item.collectionRequestId) {
              <app-arrived-patient-card
                [item]="item"
                (sendToWaiting)="onSendToWaiting($event)"
              />
            }
          </div>
        }
      </section>

      <section class="section" data-testid="preparing-section">
        <h2 class="section-title">Preparing Barcodes</h2>
        @if (service.preparing().length === 0) {
          <div data-testid="preparing-empty-state" class="empty-state">
            <p>No patients preparing.</p>
          </div>
        } @else {
          <div class="card-list">
            @for (item of service.preparing(); track item.collectionRequestId) {
              <app-preparing-patient-card
                [item]="item"
                (barcodeCreated)="onBarcodeCreated()"
              />
            }
          </div>
        }
      </section>

      @if (error()) {
        <p role="alert" class="error-text">{{ error() }}</p>
      }
    </div>
  `,
  styles: [`
    .page { max-width: 900px; margin: 2rem auto; padding: 0 1.5rem; font-family: system-ui, sans-serif; }
    .page-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 1.5rem; }
    .page-title { font-size: 1.5rem; font-weight: 600; color: #111; margin: 0; }
    .btn-refresh { padding: 0.4rem 0.9rem; background: #f3f4f6; border: 1px solid #d1d5db; border-radius: 2px; font-size: 0.8rem; cursor: pointer; color: #374151; }
    .btn-refresh:hover { background: #e5e7eb; }
    .section { margin-bottom: 2rem; }
    .section-title { font-size: 0.8rem; font-weight: 600; color: #374151; margin: 0 0 0.75rem; text-transform: uppercase; letter-spacing: 0.05em; }
    .card-list { display: flex; flex-direction: column; gap: 0.5rem; }
    .empty-state { padding: 2rem; text-align: center; color: #9ca3af; border: 1px dashed #d1d5db; }
    .error-text { color: #b91c1c; font-size: 0.875rem; margin-top: 1rem; }
  `],
})
export class TriageComponent implements OnInit {
  protected readonly service = inject(TriageService);

  protected readonly error = signal<string | null>(null);

  async ngOnInit(): Promise<void> {
    await this.refresh();
  }

  protected async refresh(): Promise<void> {
    await Promise.all([
      this.service.loadArrived(),
      this.service.loadPreparing(),
    ]);
  }

  protected async onSendToWaiting(id: string): Promise<void> {
    try {
      await this.service.moveToWaiting(id);
      await this.refresh();
    } catch (err) {
      console.error('Failed to move patient to waiting', err);
      this.error.set('Failed to send patient to waiting room.');
    }
  }

  protected async onBarcodeCreated(): Promise<void> {
    await this.service.loadPreparing();
  }
}

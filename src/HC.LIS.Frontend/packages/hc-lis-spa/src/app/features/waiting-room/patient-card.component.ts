import { Component, input, output, signal } from '@angular/core';
import { SlicePipe } from '@angular/common';
import { CollectSampleFormComponent } from './collect-sample-form.component';
import type { CollectSampleFormData } from './collect-sample-form.component';
import type { CollectionRequestSummary } from '../../core/domain/collection-request-summary';

@Component({
  selector: 'app-patient-card',
  standalone: true,
  imports: [CollectSampleFormComponent, SlicePipe],
  template: `
    <div data-testid="patient-card" class="card">
      <div class="card-row">
        <div class="card-info">
          <span class="label">ID</span>
          <span class="value mono">{{ item().patientId }}</span>
        </div>
        <div class="card-info">
          <span class="label">Arrived</span>
          <span class="value">{{ item().arrivedAt | slice:0:10 }}</span>
        </div>
        <span class="status-badge" [class]="'status-' + item().status.toLowerCase()">
          {{ item().status }}
        </span>
        <div class="actions">
          <button data-testid="call-patient-btn" class="btn-action btn-call" (click)="onCall()">
            Call
          </button>
          <button data-testid="collect-sample-btn" class="btn-action btn-collect" (click)="toggleForm()">
            {{ showForm() ? 'Cancel' : 'Collect Sample' }}
          </button>
        </div>
      </div>

      @if (showForm()) {
        <app-collect-sample-form (formSubmitted)="onFormSubmit($event)" />
      }
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
    .status-waiting { background: #fef3c7; color: #92400e; }
    .status-called { background: #dbeafe; color: #1e40af; }
    .actions { margin-left: auto; display: flex; gap: 0.5rem; }
    .btn-action { padding: 0.3rem 0.75rem; border: none; border-radius: 2px; font-size: 0.8rem; cursor: pointer; font-weight: 500; }
    .btn-call { background: #00897B; color: #fff; }
    .btn-collect { background: #0284c7; color: #fff; }
  `],
})
export class PatientCardComponent {
  readonly item = input.required<CollectionRequestSummary>();
  readonly callClicked = output<string>();
  readonly collectSubmitted = output<{ id: string; data: CollectSampleFormData }>();

  protected showForm = signal(false);

  protected onCall(): void {
    this.callClicked.emit(this.item().collectionRequestId);
  }

  protected toggleForm(): void {
    this.showForm.update(v => !v);
  }

  protected onFormSubmit(data: CollectSampleFormData): void {
    this.collectSubmitted.emit({ id: this.item().collectionRequestId, data });
    this.showForm.set(false);
  }
}

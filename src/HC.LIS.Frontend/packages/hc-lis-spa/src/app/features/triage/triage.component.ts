import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { TriageService } from './triage.service';
import { PatientRowComponent } from './patient-row.component';
import { PrintLabelsModalComponent } from './print-labels-modal.component';
import type { SampleSummary } from '../../core/domain/sample-summary';

type StatusFilter = 'All' | 'Arrived' | 'Waiting' | 'Called';

interface PrintModalRequest {
  collectionRequestId: string;
  patientId: string;
}

@Component({
  selector: 'app-triage',
  standalone: true,
  imports: [PatientRowComponent, PrintLabelsModalComponent],
  template: `
    <div class="page">
      <div class="page-header">
        <h1 data-testid="triage-title" class="page-title">Triage</h1>
        <button class="btn-refresh" (click)="refresh()">↻ Refresh</button>
      </div>

      <div class="filter-bar">
        <button data-testid="filter-tab-all"     class="tab" [class.tab-active]="activeFilter() === 'All'"     (click)="setFilter('All')">All <span class="count">{{ totalCount() }}</span></button>
        <button data-testid="filter-tab-arrived" class="tab tab-arrived" [class.tab-active]="activeFilter() === 'Arrived'" (click)="setFilter('Arrived')">Arrived <span class="count">{{ arrivedCount() }}</span></button>
        <button data-testid="filter-tab-waiting" class="tab tab-waiting" [class.tab-active]="activeFilter() === 'Waiting'" (click)="setFilter('Waiting')">Waiting <span class="count">{{ waitingCount() }}</span></button>
        <button data-testid="filter-tab-called"  class="tab tab-called"  [class.tab-active]="activeFilter() === 'Called'"  (click)="setFilter('Called')">Called  <span class="count">{{ calledCount() }}</span></button>
      </div>

      <div class="list">

        @if (showArrived()) {
          <div data-testid="arrived-group" class="group">
            <div class="group-header">Arrived</div>
            @if (service.arrived().length === 0) {
              <div class="empty-state">No patients arriving.</div>
            } @else {
              @for (item of service.arrived(); track item.collectionRequestId) {
                <app-patient-row
                  [item]="item"
                  (sendToWaiting)="onSendToWaiting($event)"
                />
              }
            }
          </div>
        }

        @if (showWaiting()) {
          <div data-testid="waiting-group" class="group">
            <div class="group-header">Waiting</div>
            @if (service.waiting().length === 0) {
              <div class="empty-state">No patients waiting.</div>
            } @else {
              @for (item of service.waiting(); track item.collectionRequestId) {
                <app-patient-row
                  [item]="item"
                  (printLabel)="onPrintLabel($event)"
                  (callPatient)="onCallPatient($event)"
                />
              }
            }
          </div>
        }

        @if (showCalled()) {
          <div data-testid="called-group" class="group">
            <div class="group-header">Called</div>
            @if (service.called().length === 0) {
              <div class="empty-state">No patients called.</div>
            } @else {
              @for (item of service.called(); track item.collectionRequestId) {
                <app-patient-row
                  [item]="item"
                  [samples]="samplesMap().get(item.collectionRequestId) ?? null"
                  (loadSamplesRequested)="onLoadSamples($event)"
                  (sampleCollectRequested)="onCollectSample($event)"
                />
              }
            }
          </div>
        }

        @if (error()) {
          <p role="alert" class="error-text">{{ error() }}</p>
        }

      </div>
    </div>

    @if (printModalRequest()) {
      <app-print-labels-modal
        [collectionRequestId]="printModalRequest()!.collectionRequestId"
        [patientId]="printModalRequest()!.patientId"
        (closed)="printModalRequest.set(null)"
      />
    }
  `,
  styles: [`
    .page { max-width: 900px; margin: 2rem auto; padding: 0 1.5rem; font-family: system-ui, sans-serif; }
    .page-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 1rem; }
    .page-title { font-size: 1.5rem; font-weight: 600; color: #111; margin: 0; }
    .btn-refresh { padding: 0.4rem 0.9rem; background: #f3f4f6; border: 1px solid #d1d5db; border-radius: 2px; font-size: 0.8rem; cursor: pointer; color: #374151; }
    .btn-refresh:hover { background: #e5e7eb; }

    .filter-bar { display: flex; gap: 0.5rem; margin-bottom: 1.25rem; flex-wrap: wrap; }
    .tab { padding: 0.3rem 0.85rem; border-radius: 20px; font-size: 0.8rem; font-weight: 500; border: 1px solid #d1d5db; background: #f9fafb; cursor: pointer; color: #374151; display: flex; align-items: center; gap: 0.35rem; }
    .tab:hover { background: #f3f4f6; }
    .tab-active { background: #1d4ed8 !important; color: #fff !important; border-color: #1d4ed8 !important; }
    .tab-arrived { border-color: #6ee7b7; color: #065f46; background: #f0fdf4; }
    .tab-waiting  { border-color: #fcd34d; color: #92400e; background: #fffbeb; }
    .tab-called   { border-color: #93c5fd; color: #1e40af; background: #eff6ff; }
    .count { font-size: 0.7rem; opacity: 0.75; }

    .list { display: flex; flex-direction: column; gap: 1.5rem; }
    .group { display: flex; flex-direction: column; gap: 0.5rem; }
    .group-header { font-size: 0.7rem; font-weight: 700; letter-spacing: 0.08em; text-transform: uppercase; color: #6b7280; margin-bottom: 0.2rem; }
    .empty-state { padding: 1.5rem; text-align: center; color: #9ca3af; border: 1px dashed #d1d5db; font-size: 0.85rem; }
    .error-text { color: #b91c1c; font-size: 0.875rem; }
  `],
})
export class TriageComponent implements OnInit {
  protected readonly service = inject(TriageService);

  protected readonly error = signal<string | null>(null);
  protected readonly activeFilter = signal<StatusFilter>('All');
  protected readonly printModalRequest = signal<PrintModalRequest | null>(null);
  protected readonly samplesMap = signal<Map<string, SampleSummary[]>>(new Map());

  protected readonly arrivedCount = computed(() => this.service.arrived().length);
  protected readonly waitingCount  = computed(() => this.service.waiting().length);
  protected readonly calledCount   = computed(() => this.service.called().length);
  protected readonly totalCount    = computed(() => this.arrivedCount() + this.waitingCount() + this.calledCount());

  protected readonly showArrived = computed(() => this.activeFilter() === 'All' || this.activeFilter() === 'Arrived');
  protected readonly showWaiting  = computed(() => this.activeFilter() === 'All' || this.activeFilter() === 'Waiting');
  protected readonly showCalled   = computed(() => this.activeFilter() === 'All' || this.activeFilter() === 'Called');

  async ngOnInit(): Promise<void> {
    await this.refresh();
  }

  protected setFilter(filter: StatusFilter): void {
    this.activeFilter.set(filter);
  }

  protected async refresh(): Promise<void> {
    await Promise.all([
      this.service.loadArrived(),
      this.service.loadWaiting(),
      this.service.loadCalled(),
    ]);
  }

  protected async onSendToWaiting(id: string): Promise<void> {
    this.error.set(null);
    try {
      await this.service.moveToWaiting(id);
      await Promise.all([this.service.loadArrived(), this.service.loadWaiting()]);
    } catch {
      this.error.set('Failed to send patient to waiting room.');
    }
  }

  protected onPrintLabel(collectionRequestId: string): void {
    this.error.set(null);
    const item = this.service.waiting().find(r => r.collectionRequestId === collectionRequestId);
    if (item) {
      this.printModalRequest.set({ collectionRequestId, patientId: item.patientId });
    }
  }

  protected async onCallPatient(id: string): Promise<void> {
    this.error.set(null);
    try {
      await this.service.callPatient(id);
      await Promise.all([this.service.loadWaiting(), this.service.loadCalled()]);
    } catch {
      this.error.set('Failed to call patient.');
    }
  }

  protected async onLoadSamples(id: string): Promise<void> {
    this.error.set(null);
    try {
      const samples = await this.service.getSamples(id);
      this.samplesMap.update(m => new Map(m).set(id, samples));
    } catch {
      this.error.set('Failed to load sample information.');
    }
  }

  protected async onCollectSample({ collectionRequestId, sampleId }: { collectionRequestId: string; sampleId: string }): Promise<void> {
    this.error.set(null);
    try {
      await this.service.recordCollection(collectionRequestId, { sampleId });
      await this.service.loadCalled();
    } catch {
      this.error.set('Failed to record collection.');
    }
  }
}

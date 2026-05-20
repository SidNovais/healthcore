import { Component, input, output, signal, computed, HostListener } from '@angular/core';
import { SlicePipe, NgClass } from '@angular/common';
import type { CollectionRequestSummary } from '../../core/domain/collection-request-summary';
import type { SampleSummary } from '../../core/domain/sample-summary';

@Component({
  selector: 'app-patient-row',
  standalone: true,
  imports: [SlicePipe, NgClass],
  template: `
    <div data-testid="patient-row" [attr.data-cr-id]="item().collectionRequestId" class="card" [ngClass]="cardClass()">
      <div class="row">
        <div class="info">
          <span class="patient-id mono">{{ item().patientId | slice:0:8 }}…</span>
          <span class="meta">{{ timestampLabel() }} · {{ item().arrivedAt | slice:11:16 }}</span>
        </div>
        <span class="badge" [class]="badgeClass()">{{ item().status }}</span>

        <div class="kebab-wrapper">
          <button
            data-testid="patient-row-menu-btn"
            class="kebab-btn"
            (click)="toggleMenu($event)"
            [attr.aria-label]="'Actions for ' + item().patientId"
          >⋮</button>

          @if (menuOpen()) {
            <div data-testid="patient-row-menu" class="kebab-menu">
              @if (item().status === 'Arrived') {
                <button data-testid="action-send-to-waiting" class="menu-item" (click)="onSendToWaiting()">
                  → Send to Waiting Room
                </button>
              }
              @if (item().status === 'Waiting') {
                <button data-testid="action-print-label" class="menu-item" (click)="onPrintLabel()">
                  🏷 Print Label
                </button>
                <hr class="divider" />
                <button data-testid="action-call-patient" class="menu-item" (click)="onCallPatient()">
                  📢 Call Patient
                </button>
              }
              @if (item().status === 'Called') {
                <button data-testid="action-record-collection" class="menu-item" (click)="onRecordCollection()">
                  ✓ Record Collection
                </button>
              }
            </div>
          }
        </div>
      </div>

      @if (pendingSamples().length > 0) {
        <div data-testid="sample-cards-panel" class="sample-cards">
          @for (sample of pendingSamples(); track sample.id) {
            <div data-testid="sample-card" class="sample-card">
              <span class="mono">{{ sample.tubeType }}</span>
              <span class="mono">{{ sample.barcode }}</span>
              <button
                data-testid="sample-collect-btn"
                class="btn-collect"
                (click)="onCollectSample(sample.id)"
              >Collect</button>
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .card { border: 1px solid #e5e7eb; border-radius: 4px; background: #fff; padding: 0.75rem 1rem; }
    .card-arrived { border-left: 3px solid #10b981; background: #fafffe; }
    .card-waiting  { border-left: 3px solid #f59e0b; background: #fffdf5; }
    .card-called   { border-left: 3px solid #3b82f6; background: #f8faff; }
    .row { display: flex; align-items: center; gap: 1rem; }
    .info { flex: 1; display: flex; flex-direction: column; gap: 0.1rem; }
    .patient-id { font-size: 0.875rem; font-weight: 700; color: #111; }
    .mono { font-family: 'JetBrains Mono', 'IBM Plex Mono', monospace; }
    .meta { font-size: 0.7rem; color: #9ca3af; }
    .badge { padding: 0.2rem 0.55rem; font-size: 0.65rem; font-weight: 700; text-transform: uppercase; letter-spacing: 0.06em; border-radius: 2px; white-space: nowrap; }
    .badge-arrived { background: #d1fae5; color: #065f46; }
    .badge-waiting  { background: #fef3c7; color: #92400e; }
    .badge-called   { background: #dbeafe; color: #1e40af; }
    .kebab-wrapper { position: relative; }
    .kebab-btn { background: none; border: 1px solid transparent; border-radius: 4px; cursor: pointer; padding: 4px 8px; color: #6b7280; font-size: 1.1rem; line-height: 1; letter-spacing: 1px; }
    .kebab-btn:hover { background: #f3f4f6; border-color: #d1d5db; color: #374151; }
    .kebab-menu { position: absolute; right: 0; top: calc(100% + 4px); background: #fff; border: 1px solid #e5e7eb; border-radius: 8px; box-shadow: 0 8px 24px rgba(0,0,0,0.12); min-width: 190px; z-index: 20; overflow: hidden; }
    .menu-item { display: flex; align-items: center; gap: 8px; padding: 10px 14px; font-size: 0.85rem; color: #374151; cursor: pointer; border: none; background: none; width: 100%; text-align: left; }
    .menu-item:hover { background: #f9fafb; }
    .divider { height: 1px; background: #f3f4f6; margin: 2px 0; border: none; }
    .sample-cards { display: flex; flex-direction: column; gap: 0.5rem; margin-top: 0.75rem; padding-top: 0.75rem; border-top: 1px solid #e5e7eb; }
    .sample-card { display: flex; align-items: center; gap: 1rem; padding: 0.5rem 0.75rem; background: #f8faff; border: 1px solid #dbeafe; border-radius: 4px; }
    .sample-card span { flex: 1; font-size: 0.8rem; color: #374151; }
    .btn-collect { padding: 0.3rem 0.75rem; background: #1d4ed8; color: #fff; border: none; border-radius: 4px; font-size: 0.8rem; cursor: pointer; }
    .btn-collect:hover { background: #1e40af; }
  `],
})
export class PatientRowComponent {
  readonly item = input.required<CollectionRequestSummary>();
  readonly samples = input<SampleSummary[] | null>(null);
  readonly pendingSamples = computed(() =>
    (this.samples() ?? []).filter(s => s.status !== 'Collected')
  );
  readonly sendToWaiting        = output<string>();
  readonly printLabel           = output<string>();
  readonly callPatient          = output<string>();
  readonly loadSamplesRequested = output<string>();
  readonly sampleCollectRequested = output<{ collectionRequestId: string; sampleId: string }>();

  protected readonly menuOpen = signal(false);

  protected cardClass(): string {
    return `card-${this.item().status.toLowerCase()}`;
  }

  protected badgeClass(): string {
    return `badge badge-${this.item().status.toLowerCase()}`;
  }

  protected timestampLabel(): string {
    const s = this.item().status;
    if (s === 'Arrived') return 'Arrived';
    if (s === 'Waiting') return 'Waiting since';
    return 'Called at';
  }

  protected toggleMenu(event: MouseEvent): void {
    event.stopPropagation();
    this.menuOpen.update(v => !v);
  }

  protected onSendToWaiting(): void {
    this.menuOpen.set(false);
    this.sendToWaiting.emit(this.item().collectionRequestId);
  }

  protected onPrintLabel(): void {
    this.menuOpen.set(false);
    this.printLabel.emit(this.item().collectionRequestId);
  }

  protected onCallPatient(): void {
    this.menuOpen.set(false);
    this.callPatient.emit(this.item().collectionRequestId);
  }

  protected onRecordCollection(): void {
    this.menuOpen.set(false);
    this.loadSamplesRequested.emit(this.item().collectionRequestId);
  }

  protected onCollectSample(sampleId: string): void {
    this.sampleCollectRequested.emit({ collectionRequestId: this.item().collectionRequestId, sampleId });
  }

  @HostListener('document:click')
  protected onDocumentClick(): void {
    if (this.menuOpen()) {
      this.menuOpen.set(false);
    }
  }
}

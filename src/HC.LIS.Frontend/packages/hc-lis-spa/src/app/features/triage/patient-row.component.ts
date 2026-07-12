import { Component, input, output, signal, computed, HostListener } from '@angular/core';
import { SlicePipe, NgClass } from '@angular/common';
import type { CollectionRequestSummary } from '../../core/domain/collection-request-summary';
import type { SampleSummary } from '../../core/domain/sample-summary';
import { HcBadge, type HcBadgeVariant } from '../../ui/badge/badge';
import { HcButton } from '../../ui/button/button';
import { HcIcon } from '../../ui/icon/icon';

const STATUS_VARIANTS: Record<string, HcBadgeVariant> = {
  Arrived: 'success',
  Waiting: 'warning',
  Called: 'accent',
};

@Component({
  selector: 'app-patient-row',
  standalone: true,
  imports: [SlicePipe, NgClass, HcBadge, HcButton, HcIcon],
  templateUrl: './patient-row.component.html',
  styleUrl: './patient-row.component.css',
})
export class PatientRowComponent {
  readonly item = input.required<CollectionRequestSummary>();
  readonly samples = input<SampleSummary[] | null>(null);
  readonly pendingSamples = computed(() =>
    (this.samples() ?? []).filter(s => s.status !== 'Collected' && s.barcode !== null)
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

  protected statusVariant(): HcBadgeVariant {
    return STATUS_VARIANTS[this.item().status] ?? 'neutral';
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

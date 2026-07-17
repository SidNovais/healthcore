import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { TriageService } from './triage.service';
import { PatientRowComponent } from './patient-row.component';
import { PrintLabelsModalComponent } from './print-labels-modal.component';
import type { SampleSummary } from '../../core/domain/sample-summary';
import { HcAlert } from '../../ui/alert/alert';
import { HcButton } from '../../ui/button/button';
import { HcIcon } from '../../ui/icon/icon';
import { HcEmpty } from '../../ui/empty/empty';
import { HcPage } from '../../ui/page/page';
import { HcTabs, HcTab } from '../../ui/tabs/tabs';

type StatusFilter = 'All' | 'Arrived' | 'Waiting' | 'Called';

interface PrintModalRequest {
  collectionRequestId: string;
  patientId: string;
}

@Component({
  selector: 'app-triage',
  standalone: true,
  imports: [
    PatientRowComponent,
    PrintLabelsModalComponent,
    HcAlert,
    HcButton,
    HcIcon,
    HcEmpty,
    HcPage,
    HcTabs,
    HcTab,
  ],
  templateUrl: './triage.component.html',
  styleUrl: './triage.component.css',
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

  protected setFilter(filter: string): void {
    this.activeFilter.set(filter as StatusFilter);
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

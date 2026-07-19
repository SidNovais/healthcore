import { Component, effect, inject, OnInit, signal, computed } from '@angular/core';
import { TriageService } from './triage.service';
import { PATIENTS_PORT } from '../../core/application/i-patients-port';
import { PatientRowComponent } from './patient-row.component';
import { PrintLabelsModalComponent } from './print-labels-modal.component';
import type { SampleSummary } from '../../core/domain/sample-summary';
import { HcAlert } from '../../ui/alert/alert';
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
  private readonly patientsPort = inject(PATIENTS_PORT);

  /** patientId → resolved patient name, so cards show a name instead of a raw id. */
  private readonly patientNames = signal<Map<string, string>>(new Map());
  private readonly resolvingPatients = new Set<string>();

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

  constructor() {
    // Resolve a display name for every patient currently on the board (initial load and
    // live arrivals alike), so a raw patient id is never shown.
    effect(() => {
      const ids = new Set<string>();
      for (const row of [...this.service.arrived(), ...this.service.waiting(), ...this.service.called()]) {
        ids.add(row.patientId);
      }
      for (const id of ids) {
        if (this.patientNames().has(id) || this.resolvingPatients.has(id)) continue;
        this.resolvingPatients.add(id);
        void this.resolvePatientName(id);
      }
    });
  }

  /** Reads the resolved name for a patient; null until the lookup completes. */
  protected patientNameFor(patientId: string): string | null {
    return this.patientNames().get(patientId) ?? null;
  }

  private async resolvePatientName(patientId: string): Promise<void> {
    try {
      const patient = await this.patientsPort.getDetails(patientId);
      this.patientNames.update(m => new Map(m).set(patientId, patient.fullName));
    } catch {
      // Leave unresolved; the row shows the friendly placeholder.
    } finally {
      this.resolvingPatients.delete(patientId);
    }
  }

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
      this.service.applyTriageChange({ op: 'move', queue: 'waiting', collectionRequestId: id, status: 'Waiting' });
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
      this.service.applyTriageChange({ op: 'move', queue: 'called', collectionRequestId: id, status: 'Called' });
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
      this.service.applyTriageChange({ op: 'remove', collectionRequestId });
    } catch {
      this.error.set('Failed to record collection.');
    }
  }
}

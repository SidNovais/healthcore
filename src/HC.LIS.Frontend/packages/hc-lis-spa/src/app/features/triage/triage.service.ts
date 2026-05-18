import { Injectable, inject, signal } from '@angular/core';
import { COLLECTION_REQUESTS_PORT } from '../../core/application/i-collection-requests-port';
import type { RecordCollectionParams } from '../../core/application/i-collection-requests-port';
import type { CollectionRequestSummary } from '../../core/domain/collection-request-summary';
import type { SampleSummary } from '../../core/domain/sample-summary';

@Injectable({ providedIn: 'root' })
export class TriageService {
  private readonly port = inject(COLLECTION_REQUESTS_PORT);

  readonly arrived = signal<CollectionRequestSummary[]>([]);
  readonly waiting = signal<CollectionRequestSummary[]>([]);
  readonly called  = signal<CollectionRequestSummary[]>([]);

  async loadArrived(): Promise<void> {
    this.arrived.set(await this.port.loadArrived());
  }

  async loadWaiting(): Promise<void> {
    this.waiting.set(await this.port.loadQueue());
  }

  async loadCalled(): Promise<void> {
    this.called.set(await this.port.loadCalled());
  }

  async moveToWaiting(id: string): Promise<void> {
    await this.port.moveToWaiting(id);
  }

  async callPatient(id: string): Promise<void> {
    await this.port.callPatient(id);
  }

  async recordCollection(id: string, params: RecordCollectionParams): Promise<void> {
    await this.port.recordCollection(id, params);
  }

  async getSamples(id: string): Promise<SampleSummary[]> {
    return this.port.getSamples(id);
  }
}

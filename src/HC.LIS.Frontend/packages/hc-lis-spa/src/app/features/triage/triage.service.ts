import { Injectable, inject, signal } from '@angular/core';
import { COLLECTION_REQUESTS_PORT } from '../../core/application/i-collection-requests-port';
import type { CollectionRequestSummary } from '../../core/domain/collection-request-summary';
import type { SampleSummary } from '../../core/domain/sample-summary';

@Injectable({ providedIn: 'root' })
export class TriageService {
  private readonly port = inject(COLLECTION_REQUESTS_PORT);

  readonly arrived = signal<CollectionRequestSummary[]>([]);
  readonly preparing = signal<CollectionRequestSummary[]>([]);

  async loadArrived(): Promise<void> {
    const items = await this.port.loadArrived();
    this.arrived.set(items);
  }

  async loadPreparing(): Promise<void> {
    const items = await this.port.loadQueue();
    this.preparing.set(items);
  }

  async moveToWaiting(id: string): Promise<void> {
    await this.port.moveToWaiting(id);
  }

  async getSamples(id: string): Promise<SampleSummary[]> {
    return this.port.getSamples(id);
  }

}

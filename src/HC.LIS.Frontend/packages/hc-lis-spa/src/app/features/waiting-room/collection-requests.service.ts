import { Injectable, inject, signal } from '@angular/core';
import { COLLECTION_REQUESTS_PORT } from '../../core/application/i-collection-requests-port';
import type { CreateBarcodeParams, RecordCollectionParams } from '../../core/application/i-collection-requests-port';
import type { CollectionRequestSummary } from '../../core/domain/collection-request-summary';

@Injectable({ providedIn: 'root' })
export class CollectionRequestsService {
  private readonly port = inject(COLLECTION_REQUESTS_PORT);

  readonly queue = signal<CollectionRequestSummary[]>([]);

  async loadQueue(): Promise<void> {
    const items = await this.port.loadQueue();
    this.queue.set(items);
  }

  async callPatient(id: string): Promise<void> {
    await this.port.callPatient(id);
  }

  async createBarcode(id: string, params: CreateBarcodeParams): Promise<void> {
    await this.port.createBarcode(id, params);
  }

  async recordCollection(id: string, params: RecordCollectionParams): Promise<void> {
    await this.port.recordCollection(id, params);
  }
}

import { Injectable, WritableSignal, inject, signal } from '@angular/core';
import { COLLECTION_REQUESTS_PORT } from '../../core/application/i-collection-requests-port';
import type { RecordCollectionParams } from '../../core/application/i-collection-requests-port';
import type { CollectionRequestSummary } from '../../core/domain/collection-request-summary';
import type { SampleSummary } from '../../core/domain/sample-summary';
import { RealtimeClient } from '../../core/infrastructure/realtime/realtime-client';

type TriageQueue = 'arrived' | 'waiting' | 'called';

/** Shape of a `triage` topic frame pushed over the live feed. */
interface TriageRealtimeMessage {
  op?: string;
  queue?: TriageQueue;
  collectionRequestId?: string;
  status?: string;
  entity?: CollectionRequestSummary;
}

@Injectable({ providedIn: 'root' })
export class TriageService {
  private readonly port = inject(COLLECTION_REQUESTS_PORT);
  private readonly realtime = inject(RealtimeClient);

  readonly arrived = signal<CollectionRequestSummary[]>([]);
  readonly waiting = signal<CollectionRequestSummary[]>([]);
  readonly called  = signal<CollectionRequestSummary[]>([]);

  constructor() {
    this.realtime.on('triage', (payload) => this.applyTriageChange(payload));
  }

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

  /**
   * Applies a live triage change in place: a new arrival is inserted, a step transition relocates
   * the existing row to the next queue (reusing its data), and a collected request drops out.
   */
  applyTriageChange(payload: unknown): void {
    const message = payload as TriageRealtimeMessage;
    switch (message.op) {
      case 'add':
        if (message.queue && message.entity) this.insert(message.queue, message.entity);
        break;
      case 'move':
        if (message.queue && message.collectionRequestId) {
          this.move(message.collectionRequestId, message.queue, message.status);
        }
        break;
      case 'remove':
        if (message.collectionRequestId) this.removeEverywhere(message.collectionRequestId);
        break;
      default:
        break;
    }
  }

  private queueSignal(queue: TriageQueue): WritableSignal<CollectionRequestSummary[]> {
    return queue === 'arrived' ? this.arrived : queue === 'waiting' ? this.waiting : this.called;
  }

  private insert(queue: TriageQueue, row: CollectionRequestSummary): void {
    this.removeEverywhere(row.collectionRequestId);
    const target = this.queueSignal(queue);
    target.set([row, ...target()]);
  }

  private move(id: string, queue: TriageQueue, status: string | undefined): void {
    const existing = this.find(id);
    if (!existing) return;
    this.insert(queue, { ...existing, status: status ?? existing.status });
  }

  private find(id: string): CollectionRequestSummary | undefined {
    return [this.arrived(), this.waiting(), this.called()]
      .flat()
      .find((row) => row.collectionRequestId === id);
  }

  private removeEverywhere(id: string): void {
    for (const queue of [this.arrived, this.waiting, this.called]) {
      const filtered = queue().filter((row) => row.collectionRequestId !== id);
      if (filtered.length !== queue().length) queue.set(filtered);
    }
  }
}

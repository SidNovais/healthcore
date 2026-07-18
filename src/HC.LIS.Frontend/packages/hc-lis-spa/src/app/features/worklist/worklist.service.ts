import { Injectable, inject, signal } from '@angular/core';
import { WORKLIST_PORT } from '../../core/application/i-worklist-port';
import type { SignReportParams } from '../../core/application/i-worklist-port';
import type { WorklistItemSummary } from '../../core/domain/worklist-item-summary';
import type { WorklistItemDetails } from '../../core/domain/worklist-item-details';
import { RealtimeClient } from '../../core/infrastructure/realtime/realtime-client';

/** Shape of a `worklist` topic frame pushed over the live feed. */
interface WorklistRealtimeMessage {
  op?: string;
  id?: string;
  status?: string;
  entity?: WorklistItemSummary;
}

@Injectable({ providedIn: 'root' })
export class WorklistService {
  private readonly port = inject(WORKLIST_PORT);
  private readonly realtime = inject(RealtimeClient);

  readonly items = signal<WorklistItemSummary[]>([]);
  readonly selectedItem = signal<WorklistItemDetails | null>(null);
  readonly loading = signal(false);

  constructor() {
    this.realtime.on('worklist', (payload) => this.applyWorklistMessage(payload));
  }

  async loadItems(): Promise<void> {
    this.loading.set(true);
    try {
      const items = await this.port.loadItems();
      this.items.set(items);
    } finally {
      this.loading.set(false);
    }
  }

  async getItemDetails(id: string): Promise<void> {
    const details = await this.port.getItemDetails(id);
    this.selectedItem.set(details);
  }

  async signReport(id: string, params: SignReportParams): Promise<void> {
    await this.port.signReport(id, params);
  }

  /** Inserts a new worklist item or replaces an existing one with the same id, in place. */
  upsertItem(item: WorklistItemSummary): void {
    const items = this.items();
    const index = items.findIndex((i) => i.id === item.id);
    if (index === -1) {
      this.items.set([item, ...items]);
      return;
    }
    const next = [...items];
    next[index] = item;
    this.items.set(next);
  }

  /** Patches only the status of a loaded worklist item; no-op if absent or unchanged. */
  updateItemStatus(id: string, status: string): void {
    const items = this.items();
    let changed = false;
    const next = items.map((item) => {
      if (item.id === id && item.status !== status) {
        changed = true;
        return { ...item, status };
      }
      return item;
    });
    if (changed) this.items.set(next);
  }

  removeItem(id: string): void {
    const items = this.items();
    const next = items.filter((item) => item.id !== id);
    if (next.length !== items.length) this.items.set(next);
  }

  private applyWorklistMessage(payload: unknown): void {
    const message = payload as WorklistRealtimeMessage;
    switch (message.op) {
      case 'add':
        if (message.entity) this.upsertItem(message.entity);
        break;
      case 'update':
        if (message.id && message.status) this.updateItemStatus(message.id, message.status);
        break;
      case 'remove':
        if (message.id) this.removeItem(message.id);
        break;
      default:
        break;
    }
  }
}

import { Injectable, inject, signal } from '@angular/core';
import { WORKLIST_PORT } from '../../core/application/i-worklist-port';
import type { SignReportParams } from '../../core/application/i-worklist-port';
import type { WorklistItemSummary } from '../../core/domain/worklist-item-summary';
import type { WorklistItemDetails } from '../../core/domain/worklist-item-details';

@Injectable({ providedIn: 'root' })
export class WorklistService {
  private readonly port = inject(WORKLIST_PORT);

  readonly items = signal<WorklistItemSummary[]>([]);
  readonly selectedItem = signal<WorklistItemDetails | null>(null);
  readonly loading = signal(false);

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
}

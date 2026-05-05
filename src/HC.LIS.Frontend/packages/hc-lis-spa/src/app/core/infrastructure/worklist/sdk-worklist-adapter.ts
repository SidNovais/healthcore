import { Injectable, inject } from '@angular/core';
import type { IWorklistPort, SignReportParams } from '../../application/i-worklist-port';
import { WORKLIST_API } from './i-worklist-api';
import type { WorklistItemSummary } from '../../domain/worklist-item-summary';
import type { WorklistItemDetails } from '../../domain/worklist-item-details';

@Injectable()
export class SdkWorklistAdapter implements IWorklistPort {
  private readonly api = inject(WORKLIST_API);

  async loadItems(): Promise<WorklistItemSummary[]> {
    return this.api.getItems();
  }

  async getItemDetails(id: string): Promise<WorklistItemDetails> {
    return this.api.getItemDetails(id);
  }

  async signReport(id: string, params: SignReportParams): Promise<void> {
    await this.api.signReport(id, params);
  }
}

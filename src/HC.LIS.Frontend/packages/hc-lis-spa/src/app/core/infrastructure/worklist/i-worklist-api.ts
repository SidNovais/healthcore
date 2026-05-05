import { InjectionToken } from '@angular/core';
import type { WorklistItemSummary } from '../../domain/worklist-item-summary';
import type { WorklistItemDetails } from '../../domain/worklist-item-details';
import type { SignReportParams } from '../../application/i-worklist-port';

export interface IWorklistApi {
  getItems(): Promise<WorklistItemSummary[]>;
  getItemDetails(id: string): Promise<WorklistItemDetails>;
  signReport(id: string, params: SignReportParams): Promise<void>;
}

export const WORKLIST_API = new InjectionToken<IWorklistApi>('WORKLIST_API');

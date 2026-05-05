import { InjectionToken } from '@angular/core';
import type { WorklistItemSummary } from '../domain/worklist-item-summary';
import type { WorklistItemDetails } from '../domain/worklist-item-details';

export interface SignReportParams {
  signature: string;
  signedBy: string;
}

export interface IWorklistPort {
  loadItems(): Promise<WorklistItemSummary[]>;
  getItemDetails(id: string): Promise<WorklistItemDetails>;
  signReport(id: string, params: SignReportParams): Promise<void>;
}

export const WORKLIST_PORT = new InjectionToken<IWorklistPort>('WORKLIST_PORT');

import { InjectionToken } from '@angular/core';
import type { CollectionRequestSummary } from '../../domain/collection-request-summary';
import type { SampleSummary } from '../../domain/sample-summary';

export interface ApiCreateBarcodeParams {
  tubeType: string;
  barcodeValue: string;
  technicianId: string;
}

export interface ApiRecordCollectionParams {
  sampleId: string;
}

export interface ICollectionRequestsApi {
  getQueue(status: string): Promise<CollectionRequestSummary[]>;
  callPatient(id: string): Promise<void>;
  createBarcode(id: string, params: ApiCreateBarcodeParams): Promise<void>;
  recordCollection(id: string, params: ApiRecordCollectionParams): Promise<void>;
  getArrived(): Promise<CollectionRequestSummary[]>;
  moveToWaiting(id: string): Promise<void>;
  getSamples(id: string): Promise<SampleSummary[]>;
}

export const COLLECTION_REQUESTS_API = new InjectionToken<ICollectionRequestsApi>('COLLECTION_REQUESTS_API');

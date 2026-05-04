import { InjectionToken } from '@angular/core';
import type { CollectionRequestSummary } from '../../domain/collection-request-summary';

export interface ApiCreateBarcodeParams {
  tubeType: string;
  barcodeValue: string;
  technicianId: string;
}

export interface ApiRecordCollectionParams {
  sampleId: string;
  technicianId: string;
  patientName: string;
  patientBirthdate: string;
  patientGender: string;
}

export interface ICollectionRequestsApi {
  getQueue(status: string): Promise<CollectionRequestSummary[]>;
  callPatient(id: string): Promise<void>;
  createBarcode(id: string, params: ApiCreateBarcodeParams): Promise<void>;
  recordCollection(id: string, params: ApiRecordCollectionParams): Promise<void>;
}

export const COLLECTION_REQUESTS_API = new InjectionToken<ICollectionRequestsApi>('COLLECTION_REQUESTS_API');

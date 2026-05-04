import { InjectionToken } from '@angular/core';
import type { CollectionRequestSummary } from '../domain/collection-request-summary';

export interface CreateBarcodeParams {
  tubeType: string;
  barcodeValue: string;
  technicianId: string;
}

export interface RecordCollectionParams {
  sampleId: string;
  patientName: string;
  patientBirthdate: string;
  patientGender: string;
  technicianId: string;
}

export interface ICollectionRequestsPort {
  loadQueue(): Promise<CollectionRequestSummary[]>;
  callPatient(id: string): Promise<void>;
  createBarcode(id: string, params: CreateBarcodeParams): Promise<void>;
  recordCollection(id: string, params: RecordCollectionParams): Promise<void>;
}

export const COLLECTION_REQUESTS_PORT = new InjectionToken<ICollectionRequestsPort>('COLLECTION_REQUESTS_PORT');

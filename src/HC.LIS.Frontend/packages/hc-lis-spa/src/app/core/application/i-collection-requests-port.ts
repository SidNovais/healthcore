import { InjectionToken } from '@angular/core';
import type { CollectionRequestSummary } from '../domain/collection-request-summary';
import type { SampleSummary } from '../domain/sample-summary';

export interface RecordCollectionParams {
  sampleId: string;
  patientName: string;
  patientBirthdate: string;
  patientGender: string;
  technicianId: string;
}

export interface ICollectionRequestsPort {
  loadQueue(): Promise<CollectionRequestSummary[]>;
  loadCalled(): Promise<CollectionRequestSummary[]>;
  callPatient(id: string): Promise<void>;
  recordCollection(id: string, params: RecordCollectionParams): Promise<void>;
  loadArrived(): Promise<CollectionRequestSummary[]>;
  moveToWaiting(id: string): Promise<void>;
  getSamples(id: string): Promise<SampleSummary[]>;
}

export const COLLECTION_REQUESTS_PORT = new InjectionToken<ICollectionRequestsPort>('COLLECTION_REQUESTS_PORT');

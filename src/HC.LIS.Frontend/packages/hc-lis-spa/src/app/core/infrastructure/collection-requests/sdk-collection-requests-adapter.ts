import { Injectable, inject } from '@angular/core';
import type { ICollectionRequestsPort, CreateBarcodeParams, RecordCollectionParams } from '../../application/i-collection-requests-port';
import { COLLECTION_REQUESTS_API } from './i-collection-requests-api';
import type { CollectionRequestSummary } from '../../domain/collection-request-summary';

@Injectable()
export class SdkCollectionRequestsAdapter implements ICollectionRequestsPort {
  private readonly api = inject(COLLECTION_REQUESTS_API);

  async loadQueue(): Promise<CollectionRequestSummary[]> {
    return this.api.getQueue('Waiting');
  }

  async callPatient(id: string): Promise<void> {
    await this.api.callPatient(id);
  }

  async createBarcode(id: string, params: CreateBarcodeParams): Promise<void> {
    await this.api.createBarcode(id, params);
  }

  async recordCollection(id: string, params: RecordCollectionParams): Promise<void> {
    await this.api.recordCollection(id, {
      sampleId: params.sampleId,
      technicianId: params.technicianId,
      patientName: params.patientName,
      patientBirthdate: params.patientBirthdate,
      patientGender: params.patientGender,
    });
  }
}

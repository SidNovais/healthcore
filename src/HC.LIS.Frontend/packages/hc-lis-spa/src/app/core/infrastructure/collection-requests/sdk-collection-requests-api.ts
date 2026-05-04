import { Injectable } from '@angular/core';
import {
  getCollectionRequestList,
  callPatient as sdkCallPatient,
  createBarcode as sdkCreateBarcode,
  recordSampleCollection,
} from '@hc-lis/api-client';
import type { ICollectionRequestsApi, ApiCreateBarcodeParams, ApiRecordCollectionParams } from './i-collection-requests-api';
import type { CollectionRequestSummary } from '../../domain/collection-request-summary';

@Injectable()
export class SdkCollectionRequestsApi implements ICollectionRequestsApi {
  async getQueue(status: string): Promise<CollectionRequestSummary[]> {
    const result = await getCollectionRequestList({ query: { status } });
    const data = (result.data ?? []) as Array<{
      collectionRequestId: string;
      patientId: string;
      status: string;
      arrivedAt: string;
    }>;
    return data.map(item => ({
      collectionRequestId: item.collectionRequestId,
      patientId: item.patientId,
      status: item.status,
      arrivedAt: item.arrivedAt,
    }));
  }

  async callPatient(id: string): Promise<void> {
    await sdkCallPatient({ path: { id } });
  }

  async createBarcode(id: string, params: ApiCreateBarcodeParams): Promise<void> {
    await sdkCreateBarcode({
      path: { id },
      body: {
        tubeType: params.tubeType,
        barcodeValue: params.barcodeValue,
        technicianId: params.technicianId,
      },
    });
  }

  async recordCollection(id: string, params: ApiRecordCollectionParams): Promise<void> {
    await recordSampleCollection({
      path: { id },
      body: {
        sampleId: params.sampleId,
        technicianId: params.technicianId,
        patientName: params.patientName,
        patientBirthdate: params.patientBirthdate as unknown as Date,
        patientGender: params.patientGender,
      },
    });
  }
}

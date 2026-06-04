import { Injectable } from '@angular/core';
import {
  getCollectionRequestList,
  callPatient as sdkCallPatient,
  recordSampleCollection,
  movePatientToWaiting as sdkMovePatientToWaiting,
  client,
} from '@hc-lis/api-client';
import type { ICollectionRequestsApi, ApiCreateBarcodeParams, ApiRecordCollectionParams } from './i-collection-requests-api';
import type { CollectionRequestSummary } from '../../domain/collection-request-summary';
import type { SampleSummary } from '../../domain/sample-summary';

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
    await sdkCallPatient({ path: { id }, body: {} });
  }

  async createBarcode(id: string, params: ApiCreateBarcodeParams): Promise<void> {
    await client.post({
      url: '/api/v1/collection-requests/{id}/create-barcode',
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
      },
    });
  }

  async getArrived(): Promise<CollectionRequestSummary[]> {
    const result = await getCollectionRequestList({ query: { status: 'Arrived' } });
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

  async moveToWaiting(id: string): Promise<void> {
    await sdkMovePatientToWaiting({ path: { id } });
  }

  async getSamples(id: string): Promise<SampleSummary[]> {
    const result = await client.get({
      url: '/api/v1/collection-requests/{id}/samples',
      path: { id },
    });
    const data = (result.data ?? []) as Array<{
      id: string;
      tubeType: string;
      barcode: string | null;
      status: string;
    }>;
    return data.map(item => ({
      id: item.id,
      tubeType: item.tubeType,
      barcode: item.barcode,
      status: item.status,
    }));
  }
}

import { Injectable } from '@angular/core';
import {
  getWorklistItemList,
  getWorklistItemDetails as sdkGetWorklistItemDetails,
  signReport as sdkSignReport,
} from '@hc-lis/api-client';
import type {
  HcLisModulesLabAnalysisApplicationWorklistItemsGetWorklistItemListWorklistItemSummaryDto,
  HcLisModulesLabAnalysisApplicationWorklistItemsGetWorklistItemDetailsWorklistItemDetailsDto,
  HcLisModulesLabAnalysisApplicationWorklistItemsGetWorklistItemDetailsAnalyteResultDto,
} from '@hc-lis/api-client';
import type { IWorklistApi } from './i-worklist-api';
import type { SignReportParams } from '../../application/i-worklist-port';
import type { WorklistItemSummary } from '../../domain/worklist-item-summary';
import type { WorklistItemDetails, AnalyteResult } from '../../domain/worklist-item-details';

type SdkWorklistItemSummaryDto = HcLisModulesLabAnalysisApplicationWorklistItemsGetWorklistItemListWorklistItemSummaryDto;
type SdkWorklistItemDetailsDto = HcLisModulesLabAnalysisApplicationWorklistItemsGetWorklistItemDetailsWorklistItemDetailsDto;
type SdkAnalyteResultDto = HcLisModulesLabAnalysisApplicationWorklistItemsGetWorklistItemDetailsAnalyteResultDto;

@Injectable()
export class SdkWorklistApi implements IWorklistApi {
  async getItems(): Promise<WorklistItemSummary[]> {
    const result = await getWorklistItemList();
    const data: SdkWorklistItemSummaryDto[] = result.data ?? [];
    return data.map(item => ({
      id: item.id ?? '',
      sampleBarcode: item.sampleBarcode ?? '',
      examCode: item.examCode ?? '',
      patientId: item.patientId ?? '',
      patientName: item.patientName ?? null,
      patientDateOfBirth: item.patientDateOfBirth ?? null,
      patientGender: item.patientGender ?? null,
      status: item.status ?? '',
      createdAt: item.createdAt ?? '',
    }));
  }

  async getItemDetails(id: string): Promise<WorklistItemDetails> {
    const result = await sdkGetWorklistItemDetails({ path: { id } });
    const d = result.data as SdkWorklistItemDetailsDto;
    const analyteResults: AnalyteResult[] = (d.analyteResults ?? []).map((r: SdkAnalyteResultDto) => ({
      id: r.id ?? '',
      analyteCode: r.analyteCode ?? '',
      resultValue: r.resultValue ?? '',
      resultUnit: r.resultUnit ?? '',
      referenceRange: r.referenceRange ?? '',
      isOutOfRange: r.isOutOfRange ?? false,
      performedById: r.performedById ?? '',
      recordedAt: r.recordedAt ?? '',
    }));
    return {
      id: d.id ?? '',
      sampleId: d.sampleId ?? '',
      sampleBarcode: d.sampleBarcode ?? '',
      examCode: d.examCode ?? '',
      patientId: d.patientId ?? '',
      patientName: d.patientName ?? null,
      patientDateOfBirth: d.patientDateOfBirth ?? null,
      patientGender: d.patientGender ?? null,
      orderId: d.orderId ?? '',
      orderItemId: d.orderItemId ?? '',
      status: d.status ?? '',
      analyteResults,
      reportPath: d.reportPath ?? null,
      completionType: d.completionType ?? null,
      createdAt: d.createdAt ?? '',
      completedAt: d.completedAt ?? null,
    };
  }

  async signReport(id: string, params: SignReportParams): Promise<void> {
    await sdkSignReport({
      path: { id },
      body: {
        signature: params.signature,
        signedBy: params.signedBy,
      },
    });
  }
}

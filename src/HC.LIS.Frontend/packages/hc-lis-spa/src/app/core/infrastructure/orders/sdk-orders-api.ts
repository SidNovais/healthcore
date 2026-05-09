import { Injectable } from '@angular/core';
import { createOrder as sdkCreateOrder, requestExam as sdkRequestExam } from '@hc-lis/api-client';
import type { HcLisApiCommonCreatedIdResponse } from '@hc-lis/api-client';
import type { IOrdersApi, OrdersCreateParams, OrdersCreateResult, OrdersRequestExamParams } from './i-orders-api';

@Injectable()
export class SdkOrdersApi implements IOrdersApi {
  async createOrder(params: OrdersCreateParams): Promise<OrdersCreateResult> {
    const result = await sdkCreateOrder({
      body: {
        orderId: params.orderId,
        patientId: params.patientId,
        requestedBy: params.requestedBy,
        orderPriority: params.orderPriority,
      },
    });
    const data = result.data as HcLisApiCommonCreatedIdResponse;
    return { id: data?.id ?? '' };
  }

  async requestExam(orderId: string, params: OrdersRequestExamParams): Promise<void> {
    await sdkRequestExam({
      path: { orderId },
      body: {
        itemId: params.itemId,
        examMnemonic: params.examMnemonic,
        specimenMnemonic: params.specimenMnemonic,
        materialType: params.materialType,
        containerType: params.containerType,
        additive: params.additive,
        processingType: params.processingType,
        storageCondition: params.storageCondition,
      },
    });
  }
}

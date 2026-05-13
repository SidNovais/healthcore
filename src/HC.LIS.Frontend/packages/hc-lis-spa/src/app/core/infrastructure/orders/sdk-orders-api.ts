import { Injectable } from '@angular/core';
import {
  createOrder as sdkCreateOrder,
  requestExam as sdkRequestExam,
  getOrderList as sdkGetOrderList,
  getOrderDetails as sdkGetOrderDetails,
  acceptExam as sdkAcceptExam,
  cancelExam as sdkCancelExam,
  rejectExam as sdkRejectExam,
  placeExamOnHold as sdkPlaceExamOnHold,
  placeExamInProgress as sdkPlaceExamInProgress,
  partiallyCompleteExam as sdkPartiallyCompleteExam,
} from '@hc-lis/api-client';
import type { HcLisApiCommonCreatedIdResponse } from '@hc-lis/api-client';
import type { IOrdersApi, OrdersCreateParams, OrdersCreateResult, OrdersRequestExamParams } from './i-orders-api';
import type { OrderListItem } from '../../domain/order-list-item';
import type { OrderDetails } from '../../domain/order-details';

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

  async getOrderList(): Promise<OrderListItem[]> {
    const result = await sdkGetOrderList();
    const items = result.data ?? [];
    return items.map((dto) => ({
      orderId: dto.orderId ?? '',
      patientId: dto.patientId ?? '',
      requestedBy: dto.requestedBy ?? '',
      orderPriority: dto.orderPriority ?? '',
      requestedAt: dto.requestedAt ?? '',
      itemCount: dto.itemCount ?? 0,
    }));
  }

  async getOrderDetails(orderId: string): Promise<OrderDetails> {
    const result = await sdkGetOrderDetails({ path: { orderId } });
    const dto = result.data;
    return {
      orderId: dto?.orderId ?? '',
      patientId: dto?.patientId ?? '',
      requestedBy: dto?.requestedBy ?? '',
      orderPriority: dto?.orderPriority ?? '',
      requestedAt: dto?.requestedAt ?? '',
      items: [],
    };
  }

  async acceptExam(orderId: string, itemId: string): Promise<void> {
    await sdkAcceptExam({ path: { orderId, itemId } });
  }

  async cancelExam(orderId: string, itemId: string): Promise<void> {
    await sdkCancelExam({ path: { orderId, itemId } });
  }

  async rejectExam(orderId: string, itemId: string, reason: string): Promise<void> {
    await sdkRejectExam({ path: { orderId, itemId }, body: { reason } });
  }

  async placeExamOnHold(orderId: string, itemId: string, reason: string): Promise<void> {
    await sdkPlaceExamOnHold({ path: { orderId, itemId }, body: { reason } });
  }

  async placeExamInProgress(orderId: string, itemId: string): Promise<void> {
    await sdkPlaceExamInProgress({ path: { orderId, itemId } });
  }

  async partiallyCompleteExam(orderId: string, itemId: string): Promise<void> {
    await sdkPartiallyCompleteExam({ path: { orderId, itemId } });
  }
}

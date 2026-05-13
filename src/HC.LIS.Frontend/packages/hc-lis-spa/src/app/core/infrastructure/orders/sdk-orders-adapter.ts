import { Injectable, inject } from '@angular/core';
import type { IOrdersPort, CreateOrderParams, RequestExamParams } from '../../application/i-orders-port';
import { ORDERS_API } from './i-orders-api';
import type { OrderSummary } from '../../domain/order-summary';
import type { OrderListItem } from '../../domain/order-list-item';
import type { OrderDetails } from '../../domain/order-details';

@Injectable()
export class SdkOrdersAdapter implements IOrdersPort {
  private readonly api = inject(ORDERS_API);

  async createOrder(params: CreateOrderParams): Promise<OrderSummary> {
    const orderId = crypto.randomUUID();
    const result = await this.api.createOrder({
      orderId,
      patientId: params.patientId,
      requestedBy: params.requestedBy,
      orderPriority: 'Routine',
    });
    return { orderId: result.id, patientId: params.patientId };
  }

  async requestExam(orderId: string, params: RequestExamParams): Promise<void> {
    await this.api.requestExam(orderId, {
      itemId: crypto.randomUUID(),
      examMnemonic: params.examMnemonic,
      specimenMnemonic: params.specimenMnemonic,
      materialType: params.materialType,
      containerType: params.containerType,
      additive: params.additive,
      processingType: params.processingType,
      storageCondition: params.storageCondition,
    });
  }

  getOrderList(): Promise<OrderListItem[]> {
    return this.api.getOrderList();
  }

  getOrderDetails(orderId: string): Promise<OrderDetails> {
    return this.api.getOrderDetails(orderId);
  }

  acceptExam(orderId: string, itemId: string): Promise<void> {
    return this.api.acceptExam(orderId, itemId);
  }

  cancelExam(orderId: string, itemId: string): Promise<void> {
    return this.api.cancelExam(orderId, itemId);
  }

  rejectExam(orderId: string, itemId: string, reason: string): Promise<void> {
    return this.api.rejectExam(orderId, itemId, reason);
  }

  placeExamOnHold(orderId: string, itemId: string, reason: string): Promise<void> {
    return this.api.placeExamOnHold(orderId, itemId, reason);
  }

  placeExamInProgress(orderId: string, itemId: string): Promise<void> {
    return this.api.placeExamInProgress(orderId, itemId);
  }

  partiallyCompleteExam(orderId: string, itemId: string): Promise<void> {
    return this.api.partiallyCompleteExam(orderId, itemId);
  }
}

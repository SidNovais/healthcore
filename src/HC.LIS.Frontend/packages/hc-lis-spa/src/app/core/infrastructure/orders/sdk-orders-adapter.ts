import { Injectable, inject } from '@angular/core';
import type { IOrdersPort, CreateOrderParams, RequestExamParams } from '../../application/i-orders-port';
import { ORDERS_API } from './i-orders-api';
import type { OrderSummary } from '../../domain/order-summary';

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
}

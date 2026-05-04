import { Injectable, inject, signal } from '@angular/core';
import { ORDERS_PORT } from '../../core/application/i-orders-port';
import type { CreateOrderParams, RequestExamParams } from '../../core/application/i-orders-port';
import type { OrderSummary } from '../../core/domain/order-summary';

@Injectable({ providedIn: 'root' })
export class OrdersService {
  private readonly port = inject(ORDERS_PORT);

  readonly order = signal<OrderSummary | null>(null);

  async createOrder(params: CreateOrderParams): Promise<void> {
    const result = await this.port.createOrder(params);
    this.order.set(result);
  }

  async requestExam(orderId: string, params: RequestExamParams): Promise<void> {
    await this.port.requestExam(orderId, params);
  }
}

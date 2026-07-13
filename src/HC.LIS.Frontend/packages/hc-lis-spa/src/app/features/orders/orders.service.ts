import { Injectable, inject, signal } from '@angular/core';
import { ORDERS_PORT } from '../../core/application/i-orders-port';
import type { CreateOrderParams, RequestExamParams } from '../../core/application/i-orders-port';
import type { OrderSummary } from '../../core/domain/order-summary';
import type { OrderListItem } from '../../core/domain/order-list-item';
import type { OrderDetails } from '../../core/domain/order-details';

@Injectable({ providedIn: 'root' })
export class OrdersService {
  private readonly port = inject(ORDERS_PORT);

  readonly order = signal<OrderSummary | null>(null);
  readonly orderList = signal<OrderListItem[]>([]);
  readonly orderDetails = signal<OrderDetails | null>(null);
  readonly loadingList = signal(false);

  async createOrder(params: CreateOrderParams): Promise<void> {
    const result = await this.port.createOrder(params);
    this.order.set(result);
  }

  async requestExam(orderId: string, params: RequestExamParams): Promise<void> {
    await this.port.requestExam(orderId, params);
  }

  async loadOrderList(): Promise<void> {
    this.loadingList.set(true);
    try {
      const items = await this.port.getOrderList();
      this.orderList.set(items);
    } finally {
      this.loadingList.set(false);
    }
  }

  async loadOrderDetails(orderId: string): Promise<void> {
    const details = await this.port.getOrderDetails(orderId);
    this.orderDetails.set(details);
  }

  async acceptExam(orderId: string, itemId: string): Promise<void> {
    await this.port.acceptExam(orderId, itemId);
  }

  async cancelExam(orderId: string, itemId: string): Promise<void> {
    await this.port.cancelExam(orderId, itemId);
  }

  async rejectExam(orderId: string, itemId: string, reason: string): Promise<void> {
    await this.port.rejectExam(orderId, itemId, reason);
  }

  async placeExamOnHold(orderId: string, itemId: string, reason: string): Promise<void> {
    await this.port.placeExamOnHold(orderId, itemId, reason);
  }

  resetOrder(): void {
    this.order.set(null);
  }

}

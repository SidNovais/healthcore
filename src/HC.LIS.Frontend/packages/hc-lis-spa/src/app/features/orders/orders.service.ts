import { Injectable, inject, signal } from '@angular/core';
import { ORDERS_PORT } from '../../core/application/i-orders-port';
import type { CreateOrderParams, RequestExamParams } from '../../core/application/i-orders-port';
import type { OrderSummary } from '../../core/domain/order-summary';
import type { OrderListItem } from '../../core/domain/order-list-item';
import type { ExamItem, ExamItemStatus, OrderDetails } from '../../core/domain/order-details';
import { RealtimeClient } from '../../core/infrastructure/realtime/realtime-client';

/** Shape of an `orders` topic frame pushed over the live feed. */
interface OrdersRealtimeMessage {
  op?: string;
  scope?: string;
  orderItemId?: string;
  status?: ExamItemStatus;
  orderId?: string;
  entity?: OrderListItem;
  item?: ExamItem;
}

@Injectable({ providedIn: 'root' })
export class OrdersService {
  private readonly port = inject(ORDERS_PORT);
  private readonly realtime = inject(RealtimeClient);

  readonly order = signal<OrderSummary | null>(null);
  readonly orderList = signal<OrderListItem[]>([]);
  readonly orderDetails = signal<OrderDetails | null>(null);
  readonly loadingList = signal(false);

  constructor() {
    this.realtime.on('orders', (payload) => this.applyOrdersMessage(payload));
  }

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

  /**
   * Patches the status/badge of an exam item in the currently loaded order detail, in place.
   * Used both for the optimistic update after the user's own action and to apply a live change
   * pushed from another client. No-op when the item is not on screen or already at that status.
   */
  applyExamStatus(orderItemId: string, status: ExamItemStatus): void {
    const details = this.orderDetails();
    if (!details) return;

    let changed = false;
    const items = details.items.map((item) => {
      if (item.orderItemId === orderItemId && item.status !== status) {
        changed = true;
        return { ...item, status };
      }
      return item;
    });

    if (changed) this.orderDetails.set({ ...details, items });
  }

  /** Inserts a newly created order at the top of the list, unless it is already there. */
  addOrderRow(row: OrderListItem): void {
    const list = this.orderList();
    if (list.some((o) => o.orderId === row.orderId)) return;
    this.orderList.set([row, ...list]);
  }

  /**
   * Reflects a newly requested exam: bumps the item count on its order-list row and, when that
   * order's detail is open, appends the new exam item. Idempotent on the item id.
   */
  applyItemAdded(orderId: string, item: ExamItem): void {
    const list = this.orderList();
    const index = list.findIndex((o) => o.orderId === orderId);
    if (index !== -1) {
      const next = [...list];
      next[index] = { ...next[index], itemCount: next[index].itemCount + 1 };
      this.orderList.set(next);
    }

    const details = this.orderDetails();
    if (details?.orderId === orderId && !details.items.some((i) => i.orderItemId === item.orderItemId)) {
      this.orderDetails.set({ ...details, items: [...details.items, item] });
    }
  }

  private applyOrdersMessage(payload: unknown): void {
    const message = payload as OrdersRealtimeMessage;
    switch (message.op) {
      case 'status':
        if (message.scope === 'exam' && message.orderItemId && message.status) {
          this.applyExamStatus(message.orderItemId, message.status);
        }
        break;
      case 'add':
        if (message.scope === 'order' && message.entity) this.addOrderRow(message.entity);
        break;
      case 'item-added':
        if (message.orderId && message.item) this.applyItemAdded(message.orderId, message.item);
        break;
      default:
        break;
    }
  }
}

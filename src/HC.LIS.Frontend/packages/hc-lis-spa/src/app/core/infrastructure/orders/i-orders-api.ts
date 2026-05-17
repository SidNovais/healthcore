import { InjectionToken } from '@angular/core';
import type { OrderListItem } from '../../domain/order-list-item';
import type { OrderDetails } from '../../domain/order-details';

export interface OrdersCreateResult {
  id: string;
}

export interface OrdersCreateParams {
  orderId: string;
  patientId: string;
  requestedBy: string;
  orderPriority: string;
}

export interface OrdersRequestExamParams {
  itemId: string;
  examMnemonic: string;
  specimenMnemonic: string;
  materialType: string;
  containerType: string;
  additive: string;
  processingType: string;
  storageCondition: string;
}

export interface IOrdersApi {
  createOrder(params: OrdersCreateParams): Promise<OrdersCreateResult>;
  requestExam(orderId: string, params: OrdersRequestExamParams): Promise<void>;
  getOrderList(): Promise<OrderListItem[]>;
  getOrderDetails(orderId: string): Promise<OrderDetails>;
  acceptExam(orderId: string, itemId: string): Promise<void>;
  cancelExam(orderId: string, itemId: string): Promise<void>;
  rejectExam(orderId: string, itemId: string, reason: string): Promise<void>;
  placeExamOnHold(orderId: string, itemId: string, reason: string): Promise<void>;
}

export const ORDERS_API = new InjectionToken<IOrdersApi>('ORDERS_API');

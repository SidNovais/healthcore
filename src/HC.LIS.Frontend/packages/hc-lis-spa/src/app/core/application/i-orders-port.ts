import { InjectionToken } from '@angular/core';
import type { OrderSummary } from '../domain/order-summary';
import type { OrderListItem } from '../domain/order-list-item';
import type { OrderDetails } from '../domain/order-details';

export interface CreateOrderParams {
  patientId: string;
  requestedBy: string;
}

export interface RequestExamParams {
  examMnemonic: string;
  specimenMnemonic: string;
  materialType: string;
  containerType: string;
  additive: string;
  processingType: string;
  storageCondition: string;
}

export interface IOrdersPort {
  createOrder(params: CreateOrderParams): Promise<OrderSummary>;
  requestExam(orderId: string, params: RequestExamParams): Promise<void>;
  getOrderList(): Promise<OrderListItem[]>;
  getOrderDetails(orderId: string): Promise<OrderDetails>;
  acceptExam(orderId: string, itemId: string): Promise<void>;
  cancelExam(orderId: string, itemId: string): Promise<void>;
  rejectExam(orderId: string, itemId: string, reason: string): Promise<void>;
  placeExamOnHold(orderId: string, itemId: string, reason: string): Promise<void>;
  placeExamInProgress(orderId: string, itemId: string): Promise<void>;
  partiallyCompleteExam(orderId: string, itemId: string): Promise<void>;
}

export const ORDERS_PORT = new InjectionToken<IOrdersPort>('ORDERS_PORT');

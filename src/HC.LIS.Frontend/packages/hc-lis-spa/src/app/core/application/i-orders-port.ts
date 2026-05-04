import { InjectionToken } from '@angular/core';
import type { OrderSummary } from '../domain/order-summary';

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
}

export const ORDERS_PORT = new InjectionToken<IOrdersPort>('ORDERS_PORT');

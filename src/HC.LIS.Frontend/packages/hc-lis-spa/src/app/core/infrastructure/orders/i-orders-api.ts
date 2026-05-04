import { InjectionToken } from '@angular/core';

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
}

export const ORDERS_API = new InjectionToken<IOrdersApi>('ORDERS_API');

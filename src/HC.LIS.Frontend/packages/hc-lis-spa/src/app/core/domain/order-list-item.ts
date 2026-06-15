export interface OrderListItem {
  orderId: string;
  patientId: string;
  patientName: string | null;
  requestedBy: string;
  orderPriority: string;
  requestedAt: string;
  itemCount: number;
}

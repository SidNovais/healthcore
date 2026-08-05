export interface OrderListItem {
  orderId: string;
  patientId: string;
  patientName: string | null;
  requestedBy: string;
  requestedByName: string | null;
  orderPriority: string;
  requestedAt: string;
  itemCount: number;
}

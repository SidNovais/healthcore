export type ExamItemStatus =
  | 'Requested'
  | 'OnHold'
  | 'Accepted'
  | 'InProgress'
  | 'PartiallyCompleted'
  | 'Completed'
  | 'Rejected'
  | 'Canceled';

export interface ExamItem {
  orderItemId: string;
  specimenMnemonic: string;
  materialType: string;
  containerType: string;
  additive: string;
  processingType: string;
  storageCondition: string;
  reasonForRejection: string | null;
  status: ExamItemStatus;
  requestedAt: string;
  canceledAt: string | null;
  onHoldAt: string | null;
  acceptedAt: string | null;
  rejectedAt: string | null;
  inProgressAt: string | null;
  partiallyCompletedAt: string | null;
  completedAt: string | null;
}

export interface OrderDetails {
  orderId: string;
  patientId: string;
  requestedBy: string;
  orderPriority: string;
  requestedAt: string;
  items: ExamItem[];
}

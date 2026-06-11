export interface AnalyteResult {
  id: string;
  analyteCode: string;
  resultValue: string;
  resultUnit: string;
  referenceRange: string;
  isOutOfRange: boolean;
  performedById: string;
  recordedAt: string;
}

export interface WorklistItemDetails {
  id: string;
  sampleId: string;
  sampleBarcode: string;
  examCode: string;
  patientId: string;
  patientName: string | null;
  patientDateOfBirth: string | null;
  patientGender: string | null;
  orderId: string;
  orderItemId: string;
  status: string;
  analyteResults: AnalyteResult[];
  reportPath: string | null;
  completionType: string | null;
  createdAt: string;
  completedAt: string | null;
}

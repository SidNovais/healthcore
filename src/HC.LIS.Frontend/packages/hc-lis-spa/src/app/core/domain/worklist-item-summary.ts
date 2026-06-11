export interface WorklistItemSummary {
  id: string;
  sampleBarcode: string;
  examCode: string;
  patientId: string;
  patientName: string | null;
  patientDateOfBirth: string | null;
  patientGender: string | null;
  status: string;
  createdAt: string;
}

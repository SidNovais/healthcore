export interface PatientDetails {
  id: string;
  fullName: string;
  dateOfBirth: string;
  gender: string | null;
  mothersFullName: string | null;
  documentId: string | null;
  phone: string | null;
  email: string | null;
  status: 'Active' | 'Anonymized';
  registeredAt: string;
  anonymizedAt: string | null;
}

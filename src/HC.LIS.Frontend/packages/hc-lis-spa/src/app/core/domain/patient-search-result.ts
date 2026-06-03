export interface PatientSearchResult {
  id: string;
  fullName: string;
  dateOfBirth: string;
  documentId: string | null;
  status: 'Active' | 'Anonymized';
}

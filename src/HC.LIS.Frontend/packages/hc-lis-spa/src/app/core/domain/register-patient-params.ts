export interface RegisterPatientParams {
  fullName: string;
  dateOfBirth: string;
  gender?: string;
  mothersFullName?: string;
  documentId?: string;
  phone?: string;
  email?: string;
}

export type UpdatePatientParams = RegisterPatientParams;

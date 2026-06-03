import { InjectionToken } from '@angular/core';
import type { PatientDetails } from '../domain/patient-details';
import type { PatientSearchResult } from '../domain/patient-search-result';
import type { RegisterPatientParams, UpdatePatientParams } from '../domain/register-patient-params';

export interface IPatientsPort {
  search(term: string): Promise<PatientSearchResult[]>;
  getDetails(id: string): Promise<PatientDetails>;
  register(data: RegisterPatientParams): Promise<string>;
  update(id: string, data: UpdatePatientParams): Promise<void>;
  anonymize(id: string): Promise<void>;
}

export const PATIENTS_PORT = new InjectionToken<IPatientsPort>('PATIENTS_PORT');

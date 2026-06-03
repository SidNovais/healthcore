import { Injectable } from '@angular/core';
import {
  searchPatients as sdkSearchPatients,
  registerPatient as sdkRegisterPatient,
  getPatientDetails as sdkGetPatientDetails,
  updatePatient as sdkUpdatePatient,
  anonymizePatient as sdkAnonymizePatient,
} from '@hc-lis/api-client';
import type { IPatientsPort } from '../../application/i-patients-port';
import type { PatientDetails } from '../../domain/patient-details';
import type { PatientSearchResult } from '../../domain/patient-search-result';
import type { RegisterPatientParams, UpdatePatientParams } from '../../domain/register-patient-params';

@Injectable()
export class SdkPatientsAdapter implements IPatientsPort {
  async search(term: string): Promise<PatientSearchResult[]> {
    const result = await sdkSearchPatients({ query: { search: term } });
    const items = (result.data ?? []) as Array<{
      id?: string;
      fullName?: string | null;
      dateOfBirth?: string;
      documentId?: string | null;
      status?: string | null;
    }>;
    return items.map(dto => ({
      id: dto.id ?? '',
      fullName: dto.fullName ?? '',
      dateOfBirth: dto.dateOfBirth ?? '',
      documentId: dto.documentId ?? null,
      status: (dto.status ?? 'Active') as 'Active' | 'Anonymized',
    }));
  }

  async getDetails(id: string): Promise<PatientDetails> {
    const result = await sdkGetPatientDetails({ path: { id } });
    const dto = result.data as {
      id?: string;
      fullName?: string | null;
      dateOfBirth?: string;
      gender?: string | null;
      mothersFullName?: string | null;
      documentId?: string | null;
      phone?: string | null;
      email?: string | null;
      status?: string | null;
      registeredAt?: string;
      anonymizedAt?: string | null;
    } | undefined;
    return {
      id: dto?.id ?? '',
      fullName: dto?.fullName ?? '',
      dateOfBirth: dto?.dateOfBirth ?? '',
      gender: dto?.gender ?? null,
      mothersFullName: dto?.mothersFullName ?? null,
      documentId: dto?.documentId ?? null,
      phone: dto?.phone ?? null,
      email: dto?.email ?? null,
      status: (dto?.status ?? 'Active') as 'Active' | 'Anonymized',
      registeredAt: dto?.registeredAt ?? '',
      anonymizedAt: dto?.anonymizedAt ?? null,
    };
  }

  async register(data: RegisterPatientParams): Promise<string> {
    const result = await sdkRegisterPatient({
      body: {
        fullName: data.fullName,
        dateOfBirth: data.dateOfBirth,
        gender: data.gender,
        mothersFullName: data.mothersFullName,
        documentId: data.documentId,
        phone: data.phone,
        email: data.email,
      },
    });
    return (result.data as { id?: string })?.id ?? '';
  }

  async update(id: string, data: UpdatePatientParams): Promise<void> {
    await sdkUpdatePatient({
      path: { id },
      body: {
        fullName: data.fullName,
        dateOfBirth: data.dateOfBirth,
        gender: data.gender,
        mothersFullName: data.mothersFullName,
        documentId: data.documentId,
        phone: data.phone,
        email: data.email,
      },
    });
  }

  async anonymize(id: string): Promise<void> {
    await sdkAnonymizePatient({ path: { id } });
  }
}

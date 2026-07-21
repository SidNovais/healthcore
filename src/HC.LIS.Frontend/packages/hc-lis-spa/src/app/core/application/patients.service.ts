import { Injectable, inject, signal } from '@angular/core';
import { PATIENTS_PORT } from './i-patients-port';
import type { PatientDetails } from '../domain/patient-details';
import type { PatientSearchResult } from '../domain/patient-search-result';
import type { RegisterPatientParams, UpdatePatientParams } from '../domain/register-patient-params';

@Injectable({ providedIn: 'root' })
export class PatientsService {
  private readonly port = inject(PATIENTS_PORT);

  readonly searchResults = signal<PatientSearchResult[]>([]);
  readonly patient = signal<PatientDetails | null>(null);
  readonly error = signal<string | null>(null);
  readonly searching = signal(false);
  readonly loadingDetails = signal(false);

  async search(term: string): Promise<void> {
    this.searching.set(true);
    try {
      const results = await this.port.search(term);
      this.searchResults.set(results);
    } catch (err) {
      this.error.set(err instanceof Error ? err.message : 'An error occurred');
    } finally {
      this.searching.set(false);
    }
  }

  /**
   * Lookup for transient callers (the command palette). Returns matches directly rather
   * than publishing them: searchResults/searching belong to the /patients page, which may
   * be rendering behind the caller — writing them would clobber the list under it.
   */
  async quickSearch(term: string): Promise<PatientSearchResult[]> {
    return this.port.search(term);
  }

  async loadDetails(id: string): Promise<void> {
    this.patient.set(null);
    this.loadingDetails.set(true);
    try {
      const details = await this.port.getDetails(id);
      this.patient.set(details);
    } catch (err) {
      this.error.set(err instanceof Error ? err.message : 'An error occurred');
    } finally {
      this.loadingDetails.set(false);
    }
  }

  async register(data: RegisterPatientParams): Promise<string> {
    return this.port.register(data);
  }

  async update(id: string, data: UpdatePatientParams): Promise<void> {
    return this.port.update(id, data);
  }

  async anonymize(id: string): Promise<void> {
    return this.port.anonymize(id);
  }
}

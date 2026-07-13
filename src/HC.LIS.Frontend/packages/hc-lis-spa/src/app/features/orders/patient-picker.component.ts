import { Component, OnDestroy, computed, inject, output, signal } from '@angular/core';
import { PATIENTS_PORT } from '../../core/application/i-patients-port';
import type { PatientSearchResult } from '../../core/domain/patient-search-result';
import { HcCombobox, type HcComboboxOption } from '../../ui/combobox/combobox';

@Component({
  selector: 'app-patient-picker',
  standalone: true,
  imports: [HcCombobox],
  templateUrl: './patient-picker.component.html',
  styleUrl: './patient-picker.component.css',
})
export class PatientPickerComponent implements OnDestroy {
  private readonly port = inject(PATIENTS_PORT);

  readonly patientSelected = output<PatientSearchResult | null>();

  protected readonly results = signal<PatientSearchResult[]>([]);
  protected readonly selectedPatient = signal<PatientSearchResult | null>(null);

  /** Adapt search results to combobox options; the document id disambiguates same-named patients. */
  protected readonly options = computed<HcComboboxOption[]>(() =>
    this.results().map(p => ({
      value: p.id,
      label: p.documentId ? `${p.fullName} · ${p.documentId}` : p.fullName,
    })),
  );

  private debounceTimer: ReturnType<typeof setTimeout> | null = null;

  protected onSearchInput(term: string): void {
    if (this.debounceTimer !== null) clearTimeout(this.debounceTimer);
    if (!term.trim()) {
      this.results.set([]);
      return;
    }
    this.debounceTimer = setTimeout(() => {
      void this.search(term);
    }, 300);
  }

  private async search(term: string): Promise<void> {
    const all = await this.port.search(term);
    this.results.set(all.filter(p => p.status !== 'Anonymized'));
  }

  protected onOptionSelected(option: HcComboboxOption): void {
    const patient = this.results().find(p => p.id === option.value);
    if (patient) {
      this.selectPatient(patient);
    }
  }

  private selectPatient(patient: PatientSearchResult): void {
    this.selectedPatient.set(patient);
    this.results.set([]);
    this.patientSelected.emit(patient);
  }

  protected clearSelection(): void {
    this.selectedPatient.set(null);
    this.results.set([]);
    this.patientSelected.emit(null);
  }

  ngOnDestroy(): void {
    if (this.debounceTimer !== null) clearTimeout(this.debounceTimer);
  }
}

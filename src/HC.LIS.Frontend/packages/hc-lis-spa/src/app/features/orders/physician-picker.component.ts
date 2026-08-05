import { Component, OnDestroy, computed, inject, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PHYSICIANS_PORT } from '../../core/application/i-physicians-port';
import type { PhysicianSearchResult } from '../../core/domain/physician-search-result';
import { HcAlert } from '../../ui/alert/alert';
import { HcButton } from '../../ui/button/button';
import { HcCombobox, type HcComboboxOption } from '../../ui/combobox/combobox';
import { HcDialog } from '../../ui/dialog/dialog';
import { HcField } from '../../ui/field/field';
import { HcInput } from '../../ui/input/input';
import { HcLabel } from '../../ui/input/label';

@Component({
  selector: 'app-physician-picker',
  standalone: true,
  imports: [FormsModule, HcAlert, HcButton, HcCombobox, HcDialog, HcField, HcInput, HcLabel],
  templateUrl: './physician-picker.component.html',
  styleUrl: './physician-picker.component.css',
})
export class PhysicianPickerComponent implements OnDestroy {
  private readonly port = inject(PHYSICIANS_PORT);

  readonly physicianSelected = output<PhysicianSearchResult | null>();

  protected readonly results = signal<PhysicianSearchResult[]>([]);
  protected readonly selectedPhysician = signal<PhysicianSearchResult | null>(null);

  /** Adapt search results to combobox options; the licence number disambiguates same-named doctors. */
  protected readonly options = computed<HcComboboxOption[]>(() =>
    this.results().map(p => ({
      value: p.id,
      label: p.licenceNumber ? `${p.fullName} · ${p.licenceNumber}` : p.fullName,
    })),
  );

  private readonly searchTerm = signal('');
  private readonly searchReturnedEmpty = signal(false);

  protected readonly canQuickAdd = computed(
    () => this.searchReturnedEmpty() && this.results().length === 0,
  );

  protected readonly quickAddOpen = signal(false);
  protected readonly quickAddPending = signal(false);
  protected readonly quickAddError = signal<string | null>(null);
  protected quickAddName = '';
  protected quickAddLicence = '';

  private debounceTimer: ReturnType<typeof setTimeout> | null = null;

  protected onSearchInput(term: string): void {
    if (this.debounceTimer !== null) clearTimeout(this.debounceTimer);
    this.searchTerm.set(term);
    if (!term.trim()) {
      this.results.set([]);
      this.searchReturnedEmpty.set(false);
      return;
    }
    this.debounceTimer = setTimeout(() => {
      void this.search(term);
    }, 300);
  }

  private async search(term: string): Promise<void> {
    const all = await this.port.search(term);
    const active = all.filter(p => p.status !== 'Inactive');
    this.results.set(active);
    this.searchReturnedEmpty.set(active.length === 0);
  }

  protected onOptionSelected(option: HcComboboxOption): void {
    const physician = this.results().find(p => p.id === option.value);
    if (physician) {
      this.selectPhysician(physician);
    }
  }

  private selectPhysician(physician: PhysicianSearchResult): void {
    this.selectedPhysician.set(physician);
    this.results.set([]);
    this.searchReturnedEmpty.set(false);
    this.physicianSelected.emit(physician);
  }

  protected clearSelection(): void {
    this.selectedPhysician.set(null);
    this.results.set([]);
    this.searchReturnedEmpty.set(false);
    this.physicianSelected.emit(null);
  }

  protected openQuickAdd(): void {
    this.quickAddName = this.searchTerm();
    this.quickAddLicence = '';
    this.quickAddError.set(null);
    this.quickAddOpen.set(true);
  }

  protected closeQuickAdd(): void {
    this.quickAddOpen.set(false);
  }

  protected async submitQuickAdd(): Promise<void> {
    const fullName = this.quickAddName.trim();
    if (!fullName) return;
    const licenceNumber = this.quickAddLicence.trim() || undefined;

    this.quickAddPending.set(true);
    this.quickAddError.set(null);
    try {
      const id = await this.port.register({ fullName, licenceNumber });
      this.quickAddOpen.set(false);
      this.selectPhysician({
        id,
        fullName,
        licenceNumber: licenceNumber ?? null,
        status: 'Active',
      });
    } catch (err) {
      this.quickAddError.set(
        err instanceof Error ? err.message : 'Failed to register the physician',
      );
    } finally {
      this.quickAddPending.set(false);
    }
  }

  ngOnDestroy(): void {
    if (this.debounceTimer !== null) clearTimeout(this.debounceTimer);
  }
}

import { Component, inject, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PatientsService } from '../../core/application/patients.service';

@Component({
  selector: 'app-patient-search',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './patient-search.component.html',
  styleUrl: './patient-search.component.css',
})
export class PatientSearchComponent implements OnDestroy {
  protected readonly service = inject(PatientsService);
  private readonly router = inject(Router);

  protected searchTerm = '';
  private debounceTimer: ReturnType<typeof setTimeout> | null = null;

  protected onSearchInput(term: string): void {
    this.searchTerm = term;
    if (this.debounceTimer !== null) {
      clearTimeout(this.debounceTimer);
    }
    this.debounceTimer = setTimeout(() => {
      void this.service.search(term);
    }, 300);
  }

  protected onRowClick(id: string): void {
    void this.router.navigate(['/patients', id]);
  }

  protected onRegisterClick(): void {
    void this.router.navigate(['/patients/new']);
  }

  ngOnDestroy(): void {
    if (this.debounceTimer !== null) {
      clearTimeout(this.debounceTimer);
    }
  }
}

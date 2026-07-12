import { Component, ElementRef, OnDestroy, effect, inject } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { gsap } from 'gsap';
import { PatientsService } from '../../core/application/patients.service';
import { HcBadge } from '../../ui/badge/badge';
import { HcButton } from '../../ui/button/button';
import { HcEmpty } from '../../ui/empty/empty';
import { HcIcon } from '../../ui/icon/icon';
import { HcInput } from '../../ui/input/input';
import { HcTable } from '../../ui/table/table';
import { MOTION, prefersReducedMotion } from '../../ui/motion/motion';

@Component({
  selector: 'app-patient-search',
  standalone: true,
  imports: [FormsModule, HcBadge, HcButton, HcEmpty, HcIcon, HcInput, HcTable],
  templateUrl: './patient-search.component.html',
  styleUrl: './patient-search.component.css',
})
export class PatientSearchComponent implements OnDestroy {
  protected readonly service = inject(PatientsService);
  private readonly router = inject(Router);
  private readonly host = inject(ElementRef).nativeElement as HTMLElement;

  protected searchTerm = '';
  private debounceTimer: ReturnType<typeof setTimeout> | null = null;

  constructor() {
    // Stagger rows in whenever a fresh (non-empty) result set renders.
    effect(() => {
      const count = this.service.searchResults().length;
      if (count === 0 || prefersReducedMotion()) return;
      requestAnimationFrame(() => {
        const rows = this.host.querySelectorAll('[data-testid="patient-row"]');
        if (rows.length === 0) return;
        gsap.from(rows, { autoAlpha: 0, y: 8, duration: MOTION.fast, stagger: 0.03, overwrite: true });
      });
    });
  }

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

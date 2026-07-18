import { Component, ElementRef, OnDestroy, computed, effect, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { gsap } from 'gsap';
import { PatientsService } from '../../core/application/patients.service';
import { HcBadge } from '../../ui/badge/badge';
import { HcButton } from '../../ui/button/button';
import { HcDatePipe } from '../../ui/date/hc-date.pipe';
import { HcEmpty } from '../../ui/empty/empty';
import { HcIcon } from '../../ui/icon/icon';
import { HcInput } from '../../ui/input/input';
import { HcPage } from '../../ui/page/page';
import { HcPagination } from '../../ui/pagination/pagination';
import {
  HcDropdownMenu,
  HcDropdownMenuItem,
  HcDropdownMenuTrigger,
} from '../../ui/dropdown-menu/dropdown-menu';
import { HcSheet } from '../../ui/sheet/sheet';
import { HcSkeleton } from '../../ui/skeleton/skeleton';
import { HcTable } from '../../ui/table/table';
import { MOTION, prefersReducedMotion } from '../../ui/motion/motion';
import { PatientDetailComponent } from './patient-detail.component';
import type { PatientSearchResult } from '../../core/domain/patient-search-result';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-patient-search',
  standalone: true,
  imports: [
    FormsModule,
    HcBadge,
    HcButton,
    HcDatePipe,
    HcEmpty,
    HcIcon,
    HcInput,
    HcPage,
    HcPagination,
    HcDropdownMenu,
    HcDropdownMenuTrigger,
    HcDropdownMenuItem,
    HcSheet,
    HcSkeleton,
    HcTable,
    PatientDetailComponent,
  ],
  templateUrl: './patient-search.component.html',
  styleUrl: './patient-search.component.css',
})
export class PatientSearchComponent implements OnDestroy {
  protected readonly service = inject(PatientsService);
  protected readonly skeletonRows = Array.from({ length: 5 });
  private readonly router = inject(Router);
  private readonly host = inject(ElementRef).nativeElement as HTMLElement;

  protected searchTerm = '';
  protected readonly page = signal(1);
  /** Patient shown in the detail slide-over; null while the sheet is closed. */
  protected readonly selectedPatientId = signal<string | null>(null);
  protected readonly detailSheetOpen = signal(false);
  private debounceTimer: ReturnType<typeof setTimeout> | null = null;

  protected readonly pageCount = computed(() =>
    Math.max(1, Math.ceil(this.service.searchResults().length / PAGE_SIZE)),
  );

  /** Current page clamped into range so a shrinking result set can't strand us. */
  protected readonly displayPage = computed(() => Math.min(this.page(), this.pageCount()));

  protected readonly pagedResults = computed<PatientSearchResult[]>(() => {
    const start = (this.displayPage() - 1) * PAGE_SIZE;
    return this.service.searchResults().slice(start, start + PAGE_SIZE);
  });

  constructor() {
    // Stagger rows in whenever a fresh (non-empty) page renders.
    effect(() => {
      const count = this.pagedResults().length;
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
    this.page.set(1);
    if (this.debounceTimer !== null) {
      clearTimeout(this.debounceTimer);
    }
    this.debounceTimer = setTimeout(() => {
      void this.service.search(term);
    }, 300);
  }

  protected clearSearch(): void {
    this.searchTerm = '';
    this.page.set(1);
    if (this.debounceTimer !== null) {
      clearTimeout(this.debounceTimer);
      this.debounceTimer = null;
    }
    void this.service.search('');
  }

  protected onRowClick(id: string): void {
    this.selectedPatientId.set(id);
    this.detailSheetOpen.set(true);
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

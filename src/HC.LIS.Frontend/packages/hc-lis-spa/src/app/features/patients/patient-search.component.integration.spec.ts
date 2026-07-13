import { TestBed } from '@angular/core/testing';
import { ComponentFixture } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { signal } from '@angular/core';
import { PatientSearchComponent } from './patient-search.component';
import { PatientsService } from '../../core/application/patients.service';
import type { PatientSearchResult } from '../../core/domain/patient-search-result';

describe('PatientSearchComponent (integration)', () => {
  let fixture: ComponentFixture<PatientSearchComponent>;
  let mockService: Partial<PatientsService>;
  let router: Router;
  let resultsSignal: ReturnType<typeof signal<PatientSearchResult[]>>;
  let searchingSignal: ReturnType<typeof signal<boolean>>;

  const twoPatients: PatientSearchResult[] = [
    { id: 'p-1', fullName: 'Maria Silva', dateOfBirth: '1985-03-12', documentId: '12345', status: 'Active' },
    { id: 'p-2', fullName: 'João Souza', dateOfBirth: '1990-07-22', documentId: null, status: 'Active' },
  ];

  beforeEach(async () => {
    resultsSignal = signal<PatientSearchResult[]>([]);
    searchingSignal = signal(false);

    mockService = {
      searchResults: resultsSignal,
      searching: searchingSignal,
      search: vi.fn().mockResolvedValue(undefined),
    };

    await TestBed.configureTestingModule({
      imports: [PatientSearchComponent],
      providers: [
        { provide: PatientsService, useValue: mockService },
        provideRouter([]),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PatientSearchComponent);
    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
    fixture.detectChanges();
  });

  afterEach(() => TestBed.resetTestingModule());

  it('renders one row per PatientSearchResult with Full Name visible', () => {
    resultsSignal.set(twoPatients);
    fixture.detectChanges();

    const rows = (fixture.nativeElement as HTMLElement).querySelectorAll('[data-testid="patient-row"]');
    expect(rows).toHaveLength(2);
    expect(rows[0].textContent).toContain('Maria Silva');
    expect(rows[1].textContent).toContain('João Souza');
  });

  it('shows empty-state element when search returns no results', () => {
    resultsSignal.set([]);
    fixture.detectChanges();

    const emptyState = (fixture.nativeElement as HTMLElement).querySelector('[data-testid="patient-search-empty-state"]');
    expect(emptyState).not.toBeNull();
  });

  it('shows skeleton rows while searching, hiding the empty-state', () => {
    searchingSignal.set(true);
    fixture.detectChanges();

    const host = fixture.nativeElement as HTMLElement;
    expect(host.querySelectorAll('[data-testid="patient-search-skeleton-row"]').length).toBeGreaterThan(0);
    expect(host.querySelector('[data-testid="patient-search-empty-state"]')).toBeNull();
  });

  it('does not show skeleton rows once the search completes', () => {
    searchingSignal.set(false);
    resultsSignal.set(twoPatients);
    fixture.detectChanges();

    const host = fixture.nativeElement as HTMLElement;
    expect(host.querySelectorAll('[data-testid="patient-search-skeleton-row"]')).toHaveLength(0);
  });

  it('clicking register-patient-btn navigates to /patients/new', async () => {
    const btn = (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>('[data-testid="register-patient-btn"]')!;
    btn.click();
    await fixture.whenStable();

    expect(router.navigate).toHaveBeenCalledWith(['/patients/new']);
  });
});

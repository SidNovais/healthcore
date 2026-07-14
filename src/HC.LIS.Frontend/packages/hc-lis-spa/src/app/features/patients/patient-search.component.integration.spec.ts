import { TestBed } from '@angular/core/testing';
import { ComponentFixture } from '@angular/core/testing';
import { ActivatedRoute, Router, provideRouter } from '@angular/router';
import { signal } from '@angular/core';
import { PatientSearchComponent } from './patient-search.component';
import { PatientsService } from '../../core/application/patients.service';
import { AuthService } from '../../core/application/auth.service';
import type { PatientSearchResult } from '../../core/domain/patient-search-result';
import type { PatientDetails } from '../../core/domain/patient-details';
import type { UserSession } from '../../core/domain/user-session';

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

  function makePatients(n: number): PatientSearchResult[] {
    return Array.from({ length: n }, (_, i) => ({
      id: `p-${i + 1}`,
      fullName: `Patient ${i + 1}`,
      dateOfBirth: '1990-01-01',
      documentId: `doc-${i + 1}`,
      status: 'Active',
    }));
  }

  function rows(): HTMLElement[] {
    return Array.from(
      (fixture.nativeElement as HTMLElement).querySelectorAll('[data-testid="patient-row"]'),
    );
  }

  beforeEach(async () => {
    resultsSignal = signal<PatientSearchResult[]>([]);
    searchingSignal = signal(false);

    mockService = {
      searchResults: resultsSignal,
      searching: searchingSignal,
      search: vi.fn().mockResolvedValue(undefined),
      // Needed by the patient-detail slide-over mounted inside the search page.
      patient: signal<PatientDetails | null>(null),
      loadDetails: vi.fn().mockResolvedValue(undefined),
      anonymize: vi.fn().mockResolvedValue(undefined),
      update: vi.fn().mockResolvedValue(undefined),
    };

    const mockAuth: Partial<AuthService> = {
      currentUser: signal<UserSession | null>(null),
    };

    await TestBed.configureTestingModule({
      imports: [PatientSearchComponent],
      providers: [
        { provide: PatientsService, useValue: mockService },
        { provide: AuthService, useValue: mockAuth },
        { provide: ActivatedRoute, useValue: { snapshot: { params: {} } } },
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

  it('paginates results, one page at a time', () => {
    resultsSignal.set(makePatients(23));
    fixture.detectChanges();

    expect(rows()).toHaveLength(10);
    const host = fixture.nativeElement as HTMLElement;
    expect(host.querySelector('[data-testid="patient-search-pagination"]')).not.toBeNull();

    host.querySelector<HTMLButtonElement>('[data-testid="patient-search-pagination-next"]')!.click();
    fixture.detectChanges();
    expect(rows()).toHaveLength(10);
    expect(rows()[0].textContent).toContain('Patient 11');
  });

  it('hides pagination when a single page of results fits', () => {
    resultsSignal.set(twoPatients);
    fixture.detectChanges();

    const host = fixture.nativeElement as HTMLElement;
    expect(host.querySelector('[data-testid="patient-search-pagination"]')).toBeNull();
  });

  it('exposes a per-row action menu whose View opens without navigating the row', () => {
    resultsSignal.set(twoPatients);
    fixture.detectChanges();

    const host = fixture.nativeElement as HTMLElement;
    const trigger = host.querySelector<HTMLButtonElement>('[data-testid="patient-actions-trigger"]')!;
    expect(trigger.getAttribute('aria-haspopup')).toBe('menu');

    trigger.click();
    fixture.detectChanges();

    expect(host.querySelector('[role="menu"]')).not.toBeNull();
    expect(host.querySelector('[data-testid="patient-action-view"]')).not.toBeNull();
    expect(rows()).toHaveLength(2);
  });

  it('shows a clear button once a term is typed and resets the search on click', () => {
    const host = fixture.nativeElement as HTMLElement;
    const input = host.querySelector<HTMLInputElement>('[data-testid="patient-search-input"]')!;

    input.value = 'Mar';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    const clear = host.querySelector<HTMLButtonElement>('[data-testid="patient-search-clear"]')!;
    expect(clear).not.toBeNull();

    clear.click();
    fixture.detectChanges();

    expect(input.value).toBe('');
    expect(mockService.search).toHaveBeenCalledWith('');
  });

  it('opens the patient-detail slide-over on row click instead of navigating', () => {
    resultsSignal.set(twoPatients);
    fixture.detectChanges();

    const host = fixture.nativeElement as HTMLElement;
    host.querySelector<HTMLElement>('[data-testid="patient-row"]')!.click();
    fixture.detectChanges();

    expect(host.querySelector('[data-testid="patient-detail-sheet"]')).not.toBeNull();
    expect(mockService.loadDetails).toHaveBeenCalledWith('p-1');
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('opens the slide-over from the row action View, not a navigation', () => {
    resultsSignal.set(twoPatients);
    fixture.detectChanges();

    const host = fixture.nativeElement as HTMLElement;
    host.querySelector<HTMLButtonElement>('[data-testid="patient-actions-trigger"]')!.click();
    fixture.detectChanges();
    host.querySelector<HTMLButtonElement>('[data-testid="patient-action-view"]')!.click();
    fixture.detectChanges();

    expect(host.querySelector('[data-testid="patient-detail-sheet"]')).not.toBeNull();
    expect(mockService.loadDetails).toHaveBeenCalledWith('p-1');
    expect(router.navigate).not.toHaveBeenCalled();
  });
});

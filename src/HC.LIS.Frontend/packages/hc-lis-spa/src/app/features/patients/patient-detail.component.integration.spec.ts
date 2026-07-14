import { TestBed, ComponentFixture } from '@angular/core/testing';
import { signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { PatientDetailComponent } from './patient-detail.component';
import { PatientsService } from '../../core/application/patients.service';
import { AuthService } from '../../core/application/auth.service';
import type { PatientDetails } from '../../core/domain/patient-details';
import type { UserSession } from '../../core/domain/user-session';

describe('PatientDetailComponent (integration)', () => {
  let fixture: ComponentFixture<PatientDetailComponent>;
  let mockPatientsService: Partial<PatientsService>;
  let mockAuthService: Partial<AuthService>;
  let patientSignal: ReturnType<typeof signal<PatientDetails | null>>;
  let currentUserSignal: ReturnType<typeof signal<UserSession | null>>;

  const patientId = 'patient-abc-123';

  const activePatient: PatientDetails = {
    id: patientId,
    fullName: 'Maria Silva',
    dateOfBirth: '1985-03-12',
    gender: 'Female',
    mothersFullName: null,
    documentId: '12345',
    phone: null,
    email: null,
    status: 'Active',
    registeredAt: '2026-06-01T10:00:00Z',
    anonymizedAt: null,
  };

  const anonymizedPatient: PatientDetails = {
    ...activePatient,
    fullName: 'ANONYMIZED',
    documentId: 'ANONYMIZED',
    status: 'Anonymized',
    anonymizedAt: '2026-06-02T10:00:00Z',
  };

  const receptionistUser: UserSession = { userId: 'u-1', userName: 'receptionist@hclis.local', role: 'Receptionist' };
  const itAdminUser: UserSession = { userId: 'u-2', userName: 'itadmin@hclis.local', role: 'ITAdmin' };

  beforeEach(async () => {
    patientSignal = signal<PatientDetails | null>(null);
    currentUserSignal = signal<UserSession | null>(null);

    mockPatientsService = {
      patient: patientSignal,
      loadDetails: vi.fn().mockResolvedValue(undefined),
      anonymize: vi.fn().mockResolvedValue(undefined),
      update: vi.fn().mockResolvedValue(undefined),
    };

    mockAuthService = {
      currentUser: currentUserSignal,
    };

    await TestBed.configureTestingModule({
      imports: [PatientDetailComponent],
      providers: [
        { provide: PatientsService, useValue: mockPatientsService },
        { provide: AuthService, useValue: mockAuthService },
        { provide: ActivatedRoute, useValue: { snapshot: { params: { id: patientId } } } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PatientDetailComponent);
    fixture.detectChanges();
  });

  afterEach(() => TestBed.resetTestingModule());

  it('Active patient — Receptionist: patient-edit-btn visible, patient-anonymize-btn absent', () => {
    currentUserSignal.set(receptionistUser);
    patientSignal.set(activePatient);
    fixture.detectChanges();

    const editBtn = (fixture.nativeElement as HTMLElement).querySelector('[data-testid="patient-edit-btn"]');
    const anonymizeBtn = (fixture.nativeElement as HTMLElement).querySelector('[data-testid="patient-anonymize-btn"]');

    expect(editBtn).not.toBeNull();
    expect(anonymizeBtn).toBeNull();
  });

  it('Active patient — ITAdmin: both patient-edit-btn and patient-anonymize-btn visible', () => {
    currentUserSignal.set(itAdminUser);
    patientSignal.set(activePatient);
    fixture.detectChanges();

    const editBtn = (fixture.nativeElement as HTMLElement).querySelector('[data-testid="patient-edit-btn"]');
    const anonymizeBtn = (fixture.nativeElement as HTMLElement).querySelector('[data-testid="patient-anonymize-btn"]');

    expect(editBtn).not.toBeNull();
    expect(anonymizeBtn).not.toBeNull();
  });

  it('Anonymized patient: patient-edit-btn absent, patient-anonymize-btn absent, status badge reads "Anonymized"', () => {
    currentUserSignal.set(itAdminUser);
    patientSignal.set(anonymizedPatient);
    fixture.detectChanges();

    const editBtn = (fixture.nativeElement as HTMLElement).querySelector('[data-testid="patient-edit-btn"]');
    const anonymizeBtn = (fixture.nativeElement as HTMLElement).querySelector('[data-testid="patient-anonymize-btn"]');
    const statusBadge = (fixture.nativeElement as HTMLElement).querySelector('[data-testid="patient-status-badge"]');

    expect(editBtn).toBeNull();
    expect(anonymizeBtn).toBeNull();
    expect(statusBadge?.textContent).toContain('Anonymized');
  });

  it('Anonymize confirmation flow: clicking patient-anonymize-btn shows anonymize-confirm-btn; clicking confirm calls service.anonymize()', async () => {
    currentUserSignal.set(itAdminUser);
    patientSignal.set(activePatient);
    fixture.detectChanges();

    const anonymizeBtn = (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>(
      '[data-testid="patient-anonymize-btn"]',
    )!;
    anonymizeBtn.click();
    fixture.detectChanges();

    const confirmBtn = (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>(
      '[data-testid="anonymize-confirm-btn"]',
    );
    expect(confirmBtn).not.toBeNull();

    confirmBtn!.click();
    await fixture.whenStable();

    expect(mockPatientsService.anonymize).toHaveBeenCalledWith(patientId);
  });

  it('renders the anonymize confirmation inside a dialog when opened', () => {
    currentUserSignal.set(itAdminUser);
    patientSignal.set(activePatient);
    fixture.detectChanges();

    (fixture.nativeElement as HTMLElement)
      .querySelector<HTMLButtonElement>('[data-testid="patient-anonymize-btn"]')!
      .click();
    fixture.detectChanges();

    const dialog = (fixture.nativeElement as HTMLElement).querySelector('[data-testid="anonymize-dialog"]');
    expect(dialog).not.toBeNull();
    expect(dialog!.textContent).toContain('Anonymize');
  });

  it('drives the detail load from the patientId input when provided (slide-over mode)', async () => {
    const sheetFixture = TestBed.createComponent(PatientDetailComponent);
    sheetFixture.componentRef.setInput('patientId', 'sheet-patient-777');
    sheetFixture.detectChanges();
    await sheetFixture.whenStable();

    expect(mockPatientsService.loadDetails).toHaveBeenCalledWith('sheet-patient-777');
  });
});

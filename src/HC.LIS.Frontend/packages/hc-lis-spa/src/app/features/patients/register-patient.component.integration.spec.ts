import { TestBed } from '@angular/core/testing';
import { ComponentFixture } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { RegisterPatientComponent } from './register-patient.component';
import { PatientsService } from '../../core/application/patients.service';

describe('RegisterPatientComponent (integration)', () => {
  let fixture: ComponentFixture<RegisterPatientComponent>;
  let mockService: Partial<PatientsService>;
  let router: Router;

  beforeEach(async () => {
    mockService = {
      register: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [RegisterPatientComponent],
      providers: [
        { provide: PatientsService, useValue: mockService },
        provideRouter([]),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(RegisterPatientComponent);
    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
    fixture.detectChanges();
  });

  afterEach(() => TestBed.resetTestingModule());

  function fillRequiredFields(fullName: string, dateOfBirth: string): void {
    const nameInput = (fixture.nativeElement as HTMLElement).querySelector<HTMLInputElement>(
      '[data-testid="patient-full-name-input"]',
    )!;
    nameInput.value = fullName;
    nameInput.dispatchEvent(new Event('input'));

    const dobInput = (fixture.nativeElement as HTMLElement).querySelector<HTMLInputElement>(
      '[data-testid="patient-dob-input"]',
    )!;
    dobInput.value = dateOfBirth;
    dobInput.dispatchEvent(new Event('input'));

    fixture.detectChanges();
  }

  it('submitting the form calls register() with form data and navigates to /patients/:newId', async () => {
    vi.mocked(mockService.register!).mockResolvedValue('new-patient-id');

    fillRequiredFields('Maria Silva', '1985-03-12');

    const submitBtn = (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>(
      '[data-testid="patient-form-submit-btn"]',
    )!;
    submitBtn.click();
    await fixture.whenStable();

    expect(mockService.register).toHaveBeenCalledWith(
      expect.objectContaining({ fullName: 'Maria Silva', dateOfBirth: '1985-03-12' }),
    );
    expect(router.navigate).toHaveBeenCalledWith(['/patients', 'new-patient-id']);
  });

  it('shows error element when register() rejects', async () => {
    vi.mocked(mockService.register!).mockRejectedValue(new Error('Server error'));

    fillRequiredFields('Maria Silva', '1985-03-12');

    const submitBtn = (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>(
      '[data-testid="patient-form-submit-btn"]',
    )!;
    submitBtn.click();
    await fixture.whenStable();
    fixture.detectChanges();

    const errorEl = (fixture.nativeElement as HTMLElement).querySelector('[data-testid="register-patient-error"]');
    expect(errorEl).not.toBeNull();
    expect(errorEl!.textContent).toContain('Server error');
  });
});

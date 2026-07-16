import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { PatientFormComponent } from './patient-form.component';

describe('PatientFormComponent (integration)', () => {
  let fixture: ComponentFixture<PatientFormComponent>;
  let host: HTMLElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PatientFormComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(PatientFormComponent);
    host = fixture.nativeElement as HTMLElement;
    fixture.detectChanges();
  });

  afterEach(() => TestBed.resetTestingModule());

  const testId = (id: string) => host.querySelector<HTMLElement>(`[data-testid="${id}"]`);

  it('groups the fields into named demographic and contact sections', () => {
    const demographics = testId('patient-form-demographics-section');
    const contact = testId('patient-form-contact-section');

    // <fieldset> + <legend> gives the grouping a real accessible name rather than
    // leaving it as a purely visual break.
    expect(demographics!.tagName).toBe('FIELDSET');
    expect(demographics!.querySelector('legend')!.textContent).toContain('Demographics');
    expect(contact!.tagName).toBe('FIELDSET');
    expect(contact!.querySelector('legend')!.textContent).toContain('Contact');

    expect(demographics!.querySelector('[data-testid="patient-full-name-input"]')).not.toBeNull();
    expect(demographics!.querySelector('[data-testid="patient-dob-input"]')).not.toBeNull();
    expect(contact!.querySelector('[data-testid="patient-phone-input"]')).not.toBeNull();
    expect(contact!.querySelector('[data-testid="patient-email-input"]')).not.toBeNull();
  });

  it('divides the sections with a separator', () => {
    const separator = testId('patient-form-section-separator');

    expect(separator).not.toBeNull();
    expect(separator!.getAttribute('role')).toBe('separator');
  });

  it('renders date of birth through hc-date-picker, keeping the original input testid', () => {
    const picker = host.querySelector('hc-date-picker');
    const input = testId('patient-dob-input') as HTMLInputElement;

    expect(picker).not.toBeNull();
    expect(picker!.contains(input)).toBe(true);
    expect(input.id).toBe('patient-dob');
    expect(testId('patient-dob-trigger')).not.toBeNull();
  });

  it('bounds date of birth at today so a future birthdate cannot be picked', () => {
    const trigger = testId('patient-dob-trigger')!;
    trigger.click();
    fixture.detectChanges();

    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    const iso = [
      tomorrow.getFullYear(),
      String(tomorrow.getMonth() + 1).padStart(2, '0'),
      String(tomorrow.getDate()).padStart(2, '0'),
    ].join('-');

    const day = testId(`patient-dob-day-${iso}`) as HTMLButtonElement | null;
    // Tomorrow is only rendered when it shares this month; either way it must not be pickable.
    expect(day?.disabled ?? true).toBe(true);
  });

  it('still emits a typed date of birth on submit', () => {
    const emitted: unknown[] = [];
    fixture.componentInstance.formSubmit.subscribe(v => emitted.push(v));

    const name = testId('patient-full-name-input') as HTMLInputElement;
    name.value = 'Maria Silva';
    name.dispatchEvent(new Event('input'));

    const dob = testId('patient-dob-input') as HTMLInputElement;
    dob.value = '1985-03-12';
    dob.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    (testId('patient-form-submit-btn') as HTMLButtonElement).click();

    expect(emitted).toEqual([
      expect.objectContaining({ fullName: 'Maria Silva', dateOfBirth: '1985-03-12' }),
    ]);
  });

  it('picking a day from the calendar fills the date of birth', () => {
    const dob = testId('patient-dob-input') as HTMLInputElement;
    dob.value = '1985-03-12';
    dob.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    testId('patient-dob-trigger')!.click();
    fixture.detectChanges();
    testId('patient-dob-day-1985-03-20')!.click();
    fixture.detectChanges();

    expect((testId('patient-dob-input') as HTMLInputElement).value).toBe('1985-03-20');
  });

  it('keeps gender on hc-select', () => {
    const gender = testId('patient-gender-select')!;

    expect(gender.tagName).toBe('SELECT');
    expect(gender.classList).toContain('hc-select');
  });
});

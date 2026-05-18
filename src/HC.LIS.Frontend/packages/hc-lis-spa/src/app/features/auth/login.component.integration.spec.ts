import { TestBed } from '@angular/core/testing';
import { ComponentFixture } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { signal } from '@angular/core';
import { LoginComponent } from './login.component';
import { AuthService } from '../../core/application/auth.service';
import type { UserSession } from '../../core/domain/user-session';

describe('LoginComponent (integration)', () => {
  let fixture: ComponentFixture<LoginComponent>;
  let mockAuthService: Partial<AuthService>;
  let router: Router;

  const receptionist: UserSession = { userId: '1', userName: 'rcpt@hclis.local', role: 'Receptionist' };
  const labTech: UserSession = { userId: '2', userName: 'lab@hclis.local', role: 'LabTechnician' };

  beforeEach(async () => {
    mockAuthService = {
      currentUser: signal<UserSession | null>(null),
      login: vi.fn(),
      me: vi.fn(),
      logout: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        { provide: AuthService, useValue: mockAuthService },
        provideRouter([]),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
    fixture.detectChanges();
  });

  afterEach(() => TestBed.resetTestingModule());

  function fillAndSubmit(email: string, password: string) {
    const compiled = fixture.nativeElement as HTMLElement;
    const emailInput = compiled.querySelector<HTMLInputElement>('[type="email"]')!;
    const passwordInput = compiled.querySelector<HTMLInputElement>('[type="password"]')!;
    const submitBtn = compiled.querySelector<HTMLButtonElement>('[type="submit"]')!;

    emailInput.value = email;
    emailInput.dispatchEvent(new Event('input'));
    passwordInput.value = password;
    passwordInput.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    submitBtn.click();
  }

  it('valid Receptionist login navigates to /orders/new', async () => {
    vi.mocked(mockAuthService.login!).mockImplementation(async () => {
      (mockAuthService as any).currentUser = signal(receptionist);
    });

    fillAndSubmit('rcpt@hclis.local', 'password123');
    await fixture.whenStable();

    expect(router.navigate).toHaveBeenCalledWith(['/orders/new']);
  });

  it('valid LabTechnician login navigates to /triage', async () => {
    vi.mocked(mockAuthService.login!).mockImplementation(async () => {
      (mockAuthService as any).currentUser = signal(labTech);
    });

    fillAndSubmit('lab@hclis.local', 'password123');
    await fixture.whenStable();

    expect(router.navigate).toHaveBeenCalledWith(['/triage']);
  });

  it('invalid credentials shows error alert; currentUser remains null', async () => {
    vi.mocked(mockAuthService.login!).mockRejectedValue(new Error('Invalid credentials'));

    fillAndSubmit('nobody@hclis.local', 'wrongpassword');
    await fixture.whenStable();
    fixture.detectChanges();

    const alert = (fixture.nativeElement as HTMLElement).querySelector('[role="alert"]');
    expect(alert).not.toBeNull();
    expect(mockAuthService.currentUser!()).toBeNull();
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('submit button is disabled when form is invalid', () => {
    const submitBtn = (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>('[type="submit"]')!;
    expect(submitBtn.disabled).toBe(true);
  });
});

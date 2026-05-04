import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { signal } from '@angular/core';
import { LoginComponent } from './login.component';
import { AuthService } from '../../core/application/auth.service';
import type { UserSession } from '../../core/domain/user-session';

describe('LoginComponent (integration)', () => {
  let mockAuthService: Partial<AuthService>;

  const receptionist: UserSession = { userId: '1', userName: 'rcpt@hclis.local', role: 'Receptionist' };
  const labTech: UserSession = { userId: '2', userName: 'lab@hclis.local', role: 'LabTechnician' };

  beforeEach(() => {
    mockAuthService = {
      currentUser: signal<UserSession | null>(null),
      login: vi.fn(),
      me: vi.fn(),
      logout: vi.fn(),
    };

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: mockAuthService },
        provideRouter([
          { path: 'login', component: LoginComponent },
          { path: 'orders/new', component: {} as any },
          { path: 'waiting-room', component: {} as any },
        ]),
      ],
    });
  });

  afterEach(() => TestBed.resetTestingModule());

  it('valid Receptionist login navigates to /orders/new', async () => {
    vi.mocked(mockAuthService.login!).mockImplementation(async () => {
      (mockAuthService as any).currentUser = signal(receptionist);
    });

    const harness = await RouterTestingHarness.create('/login');
    await harness.navigateByUrl('/login');

    const emailInput = harness.routeNativeElement!.querySelector<HTMLInputElement>('[type="email"]')!;
    const passwordInput = harness.routeNativeElement!.querySelector<HTMLInputElement>('[type="password"]')!;
    const submitBtn = harness.routeNativeElement!.querySelector<HTMLButtonElement>('[type="submit"]')!;

    emailInput.value = 'rcpt@hclis.local';
    emailInput.dispatchEvent(new Event('input'));
    passwordInput.value = 'password123';
    passwordInput.dispatchEvent(new Event('input'));

    await harness.fixture.whenStable();
    submitBtn.click();
    await harness.fixture.whenStable();

    expect(harness.routerState.url).toBe('/orders/new');
  });

  it('valid LabTechnician login navigates to /waiting-room', async () => {
    vi.mocked(mockAuthService.login!).mockImplementation(async () => {
      (mockAuthService as any).currentUser = signal(labTech);
    });

    const harness = await RouterTestingHarness.create('/login');
    await harness.navigateByUrl('/login');

    const emailInput = harness.routeNativeElement!.querySelector<HTMLInputElement>('[type="email"]')!;
    const passwordInput = harness.routeNativeElement!.querySelector<HTMLInputElement>('[type="password"]')!;
    const submitBtn = harness.routeNativeElement!.querySelector<HTMLButtonElement>('[type="submit"]')!;

    emailInput.value = 'lab@hclis.local';
    emailInput.dispatchEvent(new Event('input'));
    passwordInput.value = 'password123';
    passwordInput.dispatchEvent(new Event('input'));

    await harness.fixture.whenStable();
    submitBtn.click();
    await harness.fixture.whenStable();

    expect(harness.routerState.url).toBe('/waiting-room');
  });

  it('invalid credentials shows error alert; currentUser remains null', async () => {
    vi.mocked(mockAuthService.login!).mockRejectedValue(new Error('Invalid credentials'));

    const harness = await RouterTestingHarness.create('/login');
    await harness.navigateByUrl('/login');

    const emailInput = harness.routeNativeElement!.querySelector<HTMLInputElement>('[type="email"]')!;
    const passwordInput = harness.routeNativeElement!.querySelector<HTMLInputElement>('[type="password"]')!;
    const submitBtn = harness.routeNativeElement!.querySelector<HTMLButtonElement>('[type="submit"]')!;

    emailInput.value = 'someone@hclis.local';
    emailInput.dispatchEvent(new Event('input'));
    passwordInput.value = 'wrongpassword';
    passwordInput.dispatchEvent(new Event('input'));

    await harness.fixture.whenStable();
    submitBtn.click();
    await harness.fixture.whenStable();

    const alert = harness.routeNativeElement!.querySelector('[role="alert"]');
    expect(alert).not.toBeNull();
    expect(mockAuthService.currentUser!()).toBeNull();
    expect(harness.routerState.url).toBe('/login');
  });

  it('submit button is disabled when form is invalid', async () => {
    const harness = await RouterTestingHarness.create('/login');
    await harness.navigateByUrl('/login');

    const submitBtn = harness.routeNativeElement!.querySelector<HTMLButtonElement>('[type="submit"]')!;
    expect(submitBtn.disabled).toBe(true);
  });
});

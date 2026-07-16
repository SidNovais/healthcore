import { signal } from '@angular/core';
import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { ShellComponent } from './shell.component';
import { AuthService } from '../application/auth.service';
import { ThemeService } from '../application/theme.service';
import type { UserSession } from '../domain/user-session';

describe('ShellComponent user menu (integration)', () => {
  let fixture: ComponentFixture<ShellComponent>;
  let currentUser: ReturnType<typeof signal<UserSession | null>>;
  let mockAuth: Partial<AuthService>;
  let router: Router;

  const itAdmin: UserSession = {
    userId: 'u-1',
    userName: 'itadmin@hclis.local',
    role: 'ITAdmin',
  };

  beforeEach(async () => {
    currentUser = signal<UserSession | null>(itAdmin);
    mockAuth = {
      currentUser,
      logout: vi.fn().mockResolvedValue(undefined),
    };

    await TestBed.configureTestingModule({
      imports: [ShellComponent],
      providers: [provideRouter([]), { provide: AuthService, useValue: mockAuth }],
    }).compileComponents();

    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture = TestBed.createComponent(ShellComponent);
    fixture.detectChanges();
  });

  afterEach(() => TestBed.resetTestingModule());

  function host(): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  function byTestId<T extends HTMLElement = HTMLElement>(id: string): T | null {
    return host().querySelector<T>(`[data-testid="${id}"]`);
  }

  function openMenu(): void {
    byTestId<HTMLButtonElement>('user-menu-trigger')!.click();
    fixture.detectChanges();
  }

  it('collapses the footer affordances behind an avatar-triggered user menu', () => {
    const trigger = byTestId<HTMLButtonElement>('user-menu-trigger');

    expect(trigger).not.toBeNull();
    expect(trigger!.getAttribute('aria-haspopup')).toBe('menu');
    // Everything lives in the menu now — nothing loose in the footer until it opens.
    expect(byTestId('theme-toggle-btn')).toBeNull();
    expect(byTestId('logout-btn')).toBeNull();
    expect(byTestId('shell-role-badge')).toBeNull();
  });

  it('shows the user avatar initials on the trigger', () => {
    expect(byTestId('user-menu-avatar')?.textContent?.trim()).toBe('IT');
  });

  it('reveals the user name and role badge inside the opened menu', () => {
    openMenu();

    expect(byTestId('shell-role-badge')?.textContent?.trim()).toBe('ITAdmin');
    expect(byTestId('user-menu-name')?.textContent?.trim()).toBe('itadmin@hclis.local');
  });

  it('toggles the theme from the menu item', () => {
    const theme = TestBed.inject(ThemeService);
    const before = theme.theme();

    openMenu();
    byTestId<HTMLButtonElement>('theme-toggle-btn')!.click();
    fixture.detectChanges();

    expect(theme.theme()).not.toBe(before);
    // Activating a menu item closes the menu (dropdown contract).
    expect(byTestId('theme-toggle-btn')).toBeNull();
  });

  it('labels the theme item for the theme it switches to', () => {
    const theme = TestBed.inject(ThemeService);
    theme.theme.set('light');
    fixture.detectChanges();

    openMenu();

    expect(byTestId('theme-toggle-btn')?.getAttribute('aria-label')).toBe('Switch to dark mode');
  });

  it('signs out and returns to login from the menu item', async () => {
    openMenu();
    byTestId<HTMLButtonElement>('logout-btn')!.click();
    await fixture.whenStable();

    expect(mockAuth.logout).toHaveBeenCalledOnce();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('omits the user menu entirely when no user is signed in', () => {
    currentUser.set(null);
    fixture.detectChanges();

    expect(byTestId('user-menu-trigger')).toBeNull();
  });
});

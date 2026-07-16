import { signal } from '@angular/core';
import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { ShellComponent } from './shell.component';
import { AuthService } from '../application/auth.service';
import { PatientsService } from '../application/patients.service';
import { ThemeService } from '../application/theme.service';
import type { UserSession } from '../domain/user-session';

describe('ShellComponent user menu (integration)', () => {
  let fixture: ComponentFixture<ShellComponent>;
  let currentUser: ReturnType<typeof signal<UserSession | null>>;
  let mockAuth: Partial<AuthService>;
  let mockPatients: Partial<PatientsService>;
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
    mockPatients = { quickSearch: vi.fn().mockResolvedValue([]) };

    await TestBed.configureTestingModule({
      imports: [ShellComponent],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: mockAuth },
        { provide: PatientsService, useValue: mockPatients },
      ],
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

  describe('command palette (Phase 15)', () => {
    function pressKey(key: string, modifier: 'ctrlKey' | 'metaKey'): void {
      document.dispatchEvent(new KeyboardEvent('keydown', { key, [modifier]: true, bubbles: true }));
      fixture.detectChanges();
    }

    function type(term: string): void {
      const input = byTestId<HTMLInputElement>('command-palette-input')!;
      input.value = term;
      input.dispatchEvent(new Event('input'));
      fixture.detectChanges();
    }

    function optionLabels(): string[] {
      return Array.from(host().querySelectorAll('[role="option"]')).map(o =>
        (o.textContent ?? '').trim(),
      );
    }

    it('stays closed until the palette shortcut is pressed', () => {
      expect(byTestId('command-palette-input')).toBeNull();
    });

    it('opens on Ctrl-K', () => {
      pressKey('k', 'ctrlKey');

      expect(byTestId('command-palette-input')).not.toBeNull();
    });

    it('opens on Cmd-K for macOS', () => {
      pressKey('k', 'metaKey');

      expect(byTestId('command-palette-input')).not.toBeNull();
    });

    it('offers the signed-in role navigation destinations', () => {
      pressKey('k', 'ctrlKey');

      // ITAdmin sees all six nav destinations.
      expect(optionLabels()).toContain('Orders');
      expect(optionLabels()).toContain('Patients');
      expect(optionLabels()).toContain('Users');
    });

    it('filters the navigation destinations by the typed query', () => {
      pressKey('k', 'ctrlKey');
      type('work');

      expect(optionLabels()).toEqual(['Worklist']);
    });

    it('navigates to the destination of the chosen command', () => {
      pressKey('k', 'ctrlKey');
      type('work');
      host().querySelector<HTMLElement>('[role="option"]')!.click();
      fixture.detectChanges();

      expect(router.navigate).toHaveBeenCalledWith(['/worklist']);
    });

    it('never offers a destination the signed-in role cannot reach', () => {
      currentUser.set({ userId: 'u-2', userName: 'labtech@hclis.local', role: 'LabTechnician' });
      fixture.detectChanges();
      pressKey('k', 'ctrlKey');

      // LabTechnician's only destination is Triage.
      expect(optionLabels()).toEqual(['Triage']);
    });
  });
});

describe('ShellComponent palette patient search (integration)', () => {
  let fixture: ComponentFixture<ShellComponent>;
  let currentUser: ReturnType<typeof signal<UserSession | null>>;
  let quickSearch: ReturnType<typeof vi.fn>;
  let router: Router;

  const receptionist: UserSession = {
    userId: 'u-3',
    userName: 'receptionist@hclis.local',
    role: 'Receptionist',
  };

  beforeEach(async () => {
    vi.useFakeTimers();
    currentUser = signal<UserSession | null>(receptionist);
    quickSearch = vi.fn().mockResolvedValue([
      { id: 'p-1', fullName: 'Ada Lovelace', dateOfBirth: '1815-12-10', documentId: '42', status: 'Active' },
    ]);

    await TestBed.configureTestingModule({
      imports: [ShellComponent],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: { currentUser, logout: vi.fn() } },
        { provide: PatientsService, useValue: { quickSearch } },
      ],
    }).compileComponents();

    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture = TestBed.createComponent(ShellComponent);
    fixture.detectChanges();
  });

  afterEach(() => {
    vi.useRealTimers();
    TestBed.resetTestingModule();
  });

  function host(): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  function openAndType(term: string): void {
    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'k', ctrlKey: true, bubbles: true }));
    fixture.detectChanges();
    const input = host().querySelector<HTMLInputElement>('[data-testid="command-palette-input"]')!;
    input.value = term;
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
  }

  async function settleDebounce(): Promise<void> {
    await vi.runAllTimersAsync();
    fixture.detectChanges();
  }

  it('surfaces matching patients once the query settles', async () => {
    openAndType('ada');
    await settleDebounce();

    expect(quickSearch).toHaveBeenCalledWith('ada');
    const labels = Array.from(host().querySelectorAll('[role="option"]')).map(o =>
      (o.textContent ?? '').trim(),
    );
    expect(labels.some(l => l.includes('Ada Lovelace'))).toBe(true);
  });

  it('debounces the search to one call per settled query', async () => {
    openAndType('a');
    openAndType('ad');
    openAndType('ada');
    await settleDebounce();

    expect(quickSearch).toHaveBeenCalledTimes(1);
    expect(quickSearch).toHaveBeenCalledWith('ada');
  });

  it('does not search on a single character', async () => {
    openAndType('a');
    await settleDebounce();

    expect(quickSearch).not.toHaveBeenCalled();
  });

  it('opens the chosen patient', async () => {
    openAndType('ada');
    await settleDebounce();

    const patientOption = Array.from(host().querySelectorAll<HTMLElement>('[role="option"]')).find(
      o => (o.textContent ?? '').includes('Ada Lovelace'),
    )!;
    patientOption.click();
    fixture.detectChanges();

    expect(router.navigate).toHaveBeenCalledWith(['/patients', 'p-1']);
  });

  it('never searches patients for a role that cannot reach them', async () => {
    currentUser.set({ userId: 'u-4', userName: 'labtech@hclis.local', role: 'LabTechnician' });
    fixture.detectChanges();

    openAndType('ada');
    await settleDebounce();

    expect(quickSearch).not.toHaveBeenCalled();
  });
});

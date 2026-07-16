import {
  Component,
  DestroyRef,
  ElementRef,
  afterNextRender,
  computed,
  effect,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  NavigationEnd,
  NavigationStart,
  Router,
  RouterLink,
  RouterLinkActive,
  RouterOutlet,
} from '@angular/router';
import { filter } from 'rxjs/operators';
import { gsap } from 'gsap';
import { AuthService } from '../application/auth.service';
import { PatientsService } from '../application/patients.service';
import { ThemeService } from '../application/theme.service';
import type { UserRole } from '../domain/user-session';
import { HcAvatar } from '../../ui/avatar/avatar';
import { HcBadge } from '../../ui/badge/badge';
import { HcCommand, type HcCommandItem } from '../../ui/command/command';
import {
  HcDropdownMenu,
  HcDropdownMenuItem,
  HcDropdownMenuTrigger,
} from '../../ui/dropdown-menu/dropdown-menu';
import { HcIcon, type HcIconName } from '../../ui/icon/icon';
import { MOTION, prefersReducedMotion } from '../../ui/motion/motion';

interface NavItem {
  label: string;
  route: string;
  icon: HcIconName;
  testId: string;
}

/** A palette entry paired with the route it navigates to. */
interface PaletteCommand extends HcCommandItem {
  target: string[];
}

/** Long enough to swallow a burst of keystrokes, short enough to feel immediate. */
const PALETTE_DEBOUNCE_MS = 200;

/** Below this, a patient lookup matches too much to be useful. */
const PALETTE_MIN_QUERY = 2;

const PATIENTS_ROUTE = '/patients';

const NAV_BY_ROLE: Record<UserRole, NavItem[]> = {
  Receptionist: [
    { label: 'New Order', route: '/orders/new', icon: 'file-text', testId: 'nav-new-order-link' },
    { label: 'Orders',    route: '/orders',     icon: 'list',      testId: 'nav-orders-link'    },
    { label: 'Patients',  route: '/patients',   icon: 'user',      testId: 'nav-patients'       },
  ],
  LabTechnician: [
    { label: 'Triage', route: '/triage', icon: 'clipboard-list', testId: 'nav-triage-link' },
  ],
  Physician: [
    { label: 'Orders',   route: '/orders',   icon: 'list',  testId: 'nav-orders-link'   },
    { label: 'Worklist', route: '/worklist', icon: 'table', testId: 'nav-worklist-link' },
  ],
  ITAdmin: [
    { label: 'New Order', route: '/orders/new',  icon: 'file-text',      testId: 'nav-new-order-link' },
    { label: 'Orders',    route: '/orders',      icon: 'list',           testId: 'nav-orders-link'    },
    { label: 'Patients',  route: '/patients',    icon: 'user',           testId: 'nav-patients'       },
    { label: 'Triage',    route: '/triage',      icon: 'clipboard-list', testId: 'nav-triage-link'    },
    { label: 'Worklist',  route: '/worklist',    icon: 'table',          testId: 'nav-worklist-link'  },
    { label: 'Users',     route: '/admin/users', icon: 'users',          testId: 'nav-users-link'     },
  ],
};

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    HcAvatar,
    HcBadge,
    HcCommand,
    HcDropdownMenu,
    HcDropdownMenuTrigger,
    HcDropdownMenuItem,
    HcIcon,
  ],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.css',
  host: {
    '(document:keydown)': 'onGlobalKeydown($event)',
  },
})
export class ShellComponent {
  private readonly authService = inject(AuthService);
  private readonly patientsService = inject(PatientsService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly themeService = inject(ThemeService);

  readonly user = this.authService.currentUser;

  readonly navItems = computed<NavItem[]>(() => {
    const role = this.user()?.role;
    return role ? (NAV_BY_ROLE[role] ?? []) : [];
  });

  /** Names the theme the item switches *to* — it is both the label and the a11y name. */
  protected readonly themeLabel = computed(() =>
    this.themeService.theme() === 'dark' ? 'Switch to light mode' : 'Switch to dark mode',
  );

  protected readonly paletteOpen = signal(false);
  protected readonly paletteQuery = signal('');
  private readonly patientMatches = signal<PaletteCommand[]>([]);

  /**
   * Patient lookup is offered only to roles whose nav includes /patients — the route
   * guard would bounce anyone else, so surfacing records to them would leak PHI.
   */
  private readonly canSearchPatients = computed(() =>
    this.navItems().some(item => item.route === PATIENTS_ROUTE),
  );

  protected readonly paletteCommands = computed<PaletteCommand[]>(() => {
    const query = this.paletteQuery().trim().toLowerCase();
    const destinations = this.navItems()
      .filter(item => !query || item.label.toLowerCase().includes(query))
      .map<PaletteCommand>(item => ({
        id: `nav:${item.route}`,
        label: item.label,
        group: 'Navigation',
        target: [item.route],
      }));
    return [...destinations, ...this.patientMatches()];
  });

  private readonly outletWrapper = viewChild.required<ElementRef<HTMLElement>>('outletWrapper');

  constructor() {
    // Debounced patient lookup. Each keystroke cancels the pending timer, so only a
    // settled query reaches the API; the results are held locally rather than in
    // PatientsService's page-level signals (see quickSearch).
    effect(onCleanup => {
      const term = this.paletteQuery().trim();
      const allowed = this.canSearchPatients();

      if (!allowed || term.length < PALETTE_MIN_QUERY) {
        this.patientMatches.set([]);
        return;
      }

      const handle = setTimeout(() => void this.searchPatients(term), PALETTE_DEBOUNCE_MS);
      onCleanup(() => clearTimeout(handle));
    });

    // Route-change crossfade on the outlet wrapper. Purely cosmetic — it never blocks
    // navigation (the router resolves independently) — and is skipped entirely under
    // prefers-reduced-motion. transform/opacity only (autoAlpha), per the motion tokens.
    afterNextRender(() => {
      this.router.events
        .pipe(
          filter(e => e instanceof NavigationStart || e instanceof NavigationEnd),
          takeUntilDestroyed(this.destroyRef),
        )
        .subscribe(event => {
          if (prefersReducedMotion()) {
            return;
          }
          const el = this.outletWrapper().nativeElement;
          if (event instanceof NavigationStart) {
            gsap.to(el, { autoAlpha: 0, duration: 0.12, ease: 'power1.inOut', overwrite: 'auto' });
          } else {
            gsap.fromTo(
              el,
              { autoAlpha: 0 },
              { autoAlpha: 1, duration: MOTION.normal, ease: 'power1.inOut', overwrite: 'auto' },
            );
          }
        });
    });
  }

  async logout(): Promise<void> {
    await this.authService.logout();
    await this.router.navigate(['/login']);
  }

  protected onGlobalKeydown(event: KeyboardEvent): void {
    // Ctrl-K on Windows/Linux, Cmd-K on macOS — both, since either OS may be driving.
    if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'k') {
      event.preventDefault();
      this.paletteQuery.set('');
      this.paletteOpen.set(true);
    }
  }

  protected onPaletteSelect(item: HcCommandItem): void {
    const command = this.paletteCommands().find(c => c.id === item.id);
    if (command) {
      void this.router.navigate(command.target);
    }
  }

  private async searchPatients(term: string): Promise<void> {
    const results = await this.patientsService.quickSearch(term);
    this.patientMatches.set(
      results.map(patient => ({
        id: `patient:${patient.id}`,
        label: patient.fullName,
        group: 'Patients',
        hint: patient.dateOfBirth,
        target: [PATIENTS_ROUTE, patient.id],
      })),
    );
  }
}

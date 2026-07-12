import {
  Component,
  DestroyRef,
  ElementRef,
  afterNextRender,
  computed,
  inject,
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
import { ThemeService } from '../application/theme.service';
import type { UserRole } from '../domain/user-session';
import { HcBadge } from '../../ui/badge/badge';
import { HcButton } from '../../ui/button/button';
import { HcIcon, type HcIconName } from '../../ui/icon/icon';
import { HcTooltip } from '../../ui/tooltip/tooltip';
import { MOTION } from '../../ui/motion/motion';

interface NavItem {
  label: string;
  route: string;
  icon: HcIconName;
  testId: string;
}

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
  imports: [RouterOutlet, RouterLink, RouterLinkActive, HcBadge, HcButton, HcIcon, HcTooltip],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.css',
})
export class ShellComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly themeService = inject(ThemeService);

  readonly user = this.authService.currentUser;

  readonly navItems = computed<NavItem[]>(() => {
    const role = this.user()?.role;
    return role ? (NAV_BY_ROLE[role] ?? []) : [];
  });

  private readonly outletWrapper = viewChild.required<ElementRef<HTMLElement>>('outletWrapper');

  constructor() {
    // Route-change crossfade on the outlet wrapper. Purely cosmetic — it never blocks
    // navigation (the router resolves independently) — and is skipped entirely under
    // prefers-reduced-motion. transform/opacity only (autoAlpha), per the motion tokens.
    afterNextRender(() => {
      const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
      this.router.events
        .pipe(
          filter(e => e instanceof NavigationStart || e instanceof NavigationEnd),
          takeUntilDestroyed(this.destroyRef),
        )
        .subscribe(event => {
          if (reduceMotion.matches) {
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
}

import { Component, computed, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../application/auth.service';
import type { UserRole } from '../domain/user-session';

interface NavItem {
  label: string;
  route: string;
  icon: string;
  testId: string;
}

const NAV_BY_ROLE: Record<UserRole, NavItem[]> = {
  Receptionist: [
    { label: 'New Order', route: '/orders/new', icon: 'order', testId: 'nav-new-order-link' },
    { label: 'Orders',    route: '/orders',     icon: 'list',  testId: 'nav-orders-link'    },
  ],
  LabTechnician: [
    { label: 'Triage', route: '/triage', icon: 'triage', testId: 'nav-triage-link' },
  ],
  Physician: [
    { label: 'Orders',   route: '/orders',   icon: 'list',     testId: 'nav-orders-link'   },
    { label: 'Worklist', route: '/worklist', icon: 'worklist', testId: 'nav-worklist-link'  },
  ],
  ITAdmin: [
    { label: 'New Order', route: '/orders/new',  icon: 'order',    testId: 'nav-new-order-link'  },
    { label: 'Orders',    route: '/orders',       icon: 'list',     testId: 'nav-orders-link'     },
    { label: 'Triage',    route: '/triage',       icon: 'triage',   testId: 'nav-triage-link'     },
    { label: 'Worklist',  route: '/worklist',     icon: 'worklist', testId: 'nav-worklist-link'   },
    { label: 'Users',     route: '/admin/users',  icon: 'users',    testId: 'nav-users-link'      },
  ],
};

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.css',
})
export class ShellComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly user = this.authService.currentUser;

  readonly navItems = computed<NavItem[]>(() => {
    const role = this.user()?.role;
    return role ? (NAV_BY_ROLE[role] ?? []) : [];
  });

  async logout(): Promise<void> {
    await this.authService.logout();
    await this.router.navigate(['/login']);
  }
}

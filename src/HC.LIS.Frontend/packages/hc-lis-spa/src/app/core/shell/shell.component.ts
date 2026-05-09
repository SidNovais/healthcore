import { Component, computed, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../application/auth.service';
import type { UserRole } from '../domain/user-session';

interface NavItem {
  label: string;
  route: string;
  icon: string;
}

const NAV_BY_ROLE: Record<UserRole, NavItem[]> = {
  Receptionist: [
    { label: 'New Order', route: '/orders/new', icon: 'order' },
  ],
  LabTechnician: [
    { label: 'Waiting Room', route: '/waiting-room', icon: 'queue' },
  ],
  Physician: [
    { label: 'Worklist', route: '/worklist', icon: 'worklist' },
  ],
  ITAdmin: [
    { label: 'New Order', route: '/orders/new', icon: 'order' },
    { label: 'Waiting Room', route: '/waiting-room', icon: 'queue' },
    { label: 'Worklist', route: '/worklist', icon: 'worklist' },
    { label: 'Users', route: '/admin/users', icon: 'users' },
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

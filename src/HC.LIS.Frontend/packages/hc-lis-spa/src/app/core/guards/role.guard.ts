import { inject } from '@angular/core';
import { type CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../application/auth.service';
import type { UserRole } from '../domain/user-session';

export function roleGuard(...allowedRoles: UserRole[]): CanActivateFn {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);
    const user = authService.currentUser();

    if (!user) return router.createUrlTree(['/login']);
    if (allowedRoles.includes(user.role)) return true;
    return router.createUrlTree(['/unauthorized']);
  };
}

import { TestBed } from '@angular/core/testing';
import { Router, UrlTree, provideRouter } from '@angular/router';
import { signal } from '@angular/core';
import { roleGuard } from './role.guard';
import { AuthService } from '../application/auth.service';
import type { UserSession } from '../domain/user-session';

const mockRoute = {} as Parameters<ReturnType<typeof roleGuard>>[0];
const mockState = {} as Parameters<ReturnType<typeof roleGuard>>[1];

describe('roleGuard', () => {
  function setup(user: UserSession | null) {
    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: { currentUser: signal(user) } },
        provideRouter([]),
      ],
    });
  }

  afterEach(() => TestBed.resetTestingModule());

  it('redirects to /login when not authenticated', () => {
    setup(null);
    const guard = roleGuard('Receptionist');

    const result = TestBed.runInInjectionContext(() => guard(mockRoute, mockState));

    expect(result instanceof UrlTree).toBe(true);
    expect(TestBed.inject(Router).serializeUrl(result as UrlTree)).toBe('/login');
  });

  it('redirects to /unauthorized when role does not match', () => {
    setup({ userId: '1', userName: 'doc@hclis.local', role: 'Physician' });
    const guard = roleGuard('Receptionist');

    const result = TestBed.runInInjectionContext(() => guard(mockRoute, mockState));

    expect(result instanceof UrlTree).toBe(true);
    expect(TestBed.inject(Router).serializeUrl(result as UrlTree)).toBe('/unauthorized');
  });

  it('returns true when role matches', () => {
    setup({ userId: '1', userName: 'rcpt@hclis.local', role: 'Receptionist' });
    const guard = roleGuard('Receptionist');

    const result = TestBed.runInInjectionContext(() => guard(mockRoute, mockState));

    expect(result).toBe(true);
  });
});

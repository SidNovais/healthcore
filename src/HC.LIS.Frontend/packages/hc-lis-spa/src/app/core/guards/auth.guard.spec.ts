import { TestBed } from '@angular/core/testing';
import { Router, UrlTree, provideRouter } from '@angular/router';
import { signal } from '@angular/core';
import { authGuard } from './auth.guard';
import { AuthService } from '../application/auth.service';
import type { UserSession } from '../domain/user-session';

const mockRoute = {} as Parameters<typeof authGuard>[0];
const mockState = {} as Parameters<typeof authGuard>[1];

describe('authGuard', () => {
  function setup(user: UserSession | null) {
    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: { currentUser: signal(user) } },
        provideRouter([]),
      ],
    });
  }

  afterEach(() => TestBed.resetTestingModule());

  it('redirects to /login when currentUser is null', () => {
    setup(null);

    const result = TestBed.runInInjectionContext(() => authGuard(mockRoute, mockState));

    expect(result instanceof UrlTree).toBe(true);
    expect(TestBed.inject(Router).serializeUrl(result as UrlTree)).toBe('/login');
  });

  it('returns true when currentUser is populated', () => {
    setup({ userId: '1', userName: 'doc@hclis.local', role: 'Physician' });

    const result = TestBed.runInInjectionContext(() => authGuard(mockRoute, mockState));

    expect(result).toBe(true);
  });
});

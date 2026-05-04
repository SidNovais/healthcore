import { Injectable, inject, signal } from '@angular/core';
import { AUTH_PORT, LoginCredentials } from './i-auth-port';
import type { UserSession } from '../domain/user-session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly port = inject(AUTH_PORT);

  readonly currentUser = signal<UserSession | null>(null);

  async login(credentials: LoginCredentials): Promise<void> {
    const session = await this.port.login(credentials);
    this.currentUser.set(session);
  }

  async me(): Promise<void> {
    try {
      const session = await this.port.me();
      this.currentUser.set(session);
    } catch {
      this.currentUser.set(null);
    }
  }

  async logout(): Promise<void> {
    await this.port.logout();
    this.currentUser.set(null);
  }
}

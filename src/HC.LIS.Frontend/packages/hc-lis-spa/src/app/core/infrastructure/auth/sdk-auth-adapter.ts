import { Injectable, inject } from '@angular/core';
import type { IAuthPort, LoginCredentials } from '../../application/i-auth-port';
import { AUTH_API } from './i-auth-api';
import type { UserSession, UserRole } from '../../domain/user-session';

@Injectable()
export class SdkAuthAdapter implements IAuthPort {
  private readonly api = inject(AUTH_API);

  async login(credentials: LoginCredentials): Promise<UserSession> {
    const result = await this.api.login(credentials);
    return {
      userId: result.userId,
      userName: result.userEmail,
      role: result.role as UserRole,
    };
  }

  async me(): Promise<UserSession> {
    const result = await this.api.me();
    return {
      userId: result.userId,
      userName: result.userName,
      role: result.role as UserRole,
    };
  }

  async logout(): Promise<void> {
    // No logout endpoint in HC.LIS.API — HttpOnly cookie expires server-side.
    // The client-side session is cleared by AuthService.currentUser.set(null).
  }
}

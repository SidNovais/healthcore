import { InjectionToken } from '@angular/core';
import type { LoginCredentials } from '../../application/i-auth-port';

export interface AuthLoginResult {
  userId: string;
  userEmail: string;
  role: string;
}

export interface AuthMeResult {
  userId: string;
  userName: string;
  role: string;
}

export interface IAuthApi {
  login(credentials: LoginCredentials): Promise<AuthLoginResult>;
  me(): Promise<AuthMeResult>;
}

export const AUTH_API = new InjectionToken<IAuthApi>('AUTH_API');

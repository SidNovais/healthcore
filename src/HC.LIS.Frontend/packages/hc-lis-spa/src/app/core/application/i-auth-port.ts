import { InjectionToken } from '@angular/core';
import type { UserSession } from '../domain/user-session';

export interface LoginCredentials {
  email: string;
  password: string;
}

export interface IAuthPort {
  login(credentials: LoginCredentials): Promise<UserSession>;
  me(): Promise<UserSession>;
  logout(): Promise<void>;
}

export const AUTH_PORT = new InjectionToken<IAuthPort>('AUTH_PORT');

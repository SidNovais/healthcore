import { InjectionToken } from '@angular/core';
import type { UserSummary } from '../domain/user-summary';
import type { UserRole } from '../domain/user-session';

export interface CreateUserParams {
  email: string;
  fullName: string;
  birthdate: string;
  gender: string;
  role: UserRole;
}

export interface IUsersPort {
  listUsers(): Promise<UserSummary[]>;
  createUser(data: CreateUserParams): Promise<string>;
  changeRole(userId: string, role: UserRole): Promise<void>;
}

export const USERS_PORT = new InjectionToken<IUsersPort>('USERS_PORT');

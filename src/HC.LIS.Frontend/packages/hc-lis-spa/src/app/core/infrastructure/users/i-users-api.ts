import { InjectionToken } from '@angular/core';
import type { UserSummary } from '../../domain/user-summary';
import type { UserRole } from '../../domain/user-session';
import type { CreateUserParams } from '../../application/i-users-port';

export interface IUsersApi {
  listUsers(): Promise<UserSummary[]>;
  createUser(data: CreateUserParams): Promise<string>;
  changeRole(userId: string, role: UserRole): Promise<void>;
}

export const USERS_API = new InjectionToken<IUsersApi>('USERS_API');

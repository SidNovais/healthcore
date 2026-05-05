import { Injectable, inject } from '@angular/core';
import type { IUsersPort, CreateUserParams } from '../../application/i-users-port';
import { USERS_API } from './i-users-api';
import type { UserSummary } from '../../domain/user-summary';
import type { UserRole } from '../../domain/user-session';

@Injectable()
export class SdkUsersAdapter implements IUsersPort {
  private readonly api = inject(USERS_API);

  async listUsers(): Promise<UserSummary[]> {
    return this.api.listUsers();
  }

  async createUser(data: CreateUserParams): Promise<string> {
    return this.api.createUser(data);
  }

  async changeRole(userId: string, role: UserRole): Promise<void> {
    await this.api.changeRole(userId, role);
  }
}

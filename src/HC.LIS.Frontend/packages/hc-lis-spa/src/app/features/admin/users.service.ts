import { Injectable, inject, signal } from '@angular/core';
import { USERS_PORT } from '../../core/application/i-users-port';
import type { CreateUserParams } from '../../core/application/i-users-port';
import type { UserSummary } from '../../core/domain/user-summary';
import type { UserRole } from '../../core/domain/user-session';

@Injectable({ providedIn: 'root' })
export class UsersService {
  private readonly port = inject(USERS_PORT);

  readonly users = signal<UserSummary[]>([]);
  readonly loading = signal(false);

  async listUsers(): Promise<void> {
    this.loading.set(true);
    try {
      const users = await this.port.listUsers();
      this.users.set(users);
    } finally {
      this.loading.set(false);
    }
  }

  async createUser(data: CreateUserParams): Promise<void> {
    await this.port.createUser(data);
    await this.listUsers();
  }

  async changeRole(userId: string, role: UserRole): Promise<void> {
    await this.port.changeRole(userId, role);
    await this.listUsers();
  }
}

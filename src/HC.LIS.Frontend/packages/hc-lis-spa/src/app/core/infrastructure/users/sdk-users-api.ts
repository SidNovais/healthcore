import { Injectable } from '@angular/core';
import {
  getUserList,
  createUser as sdkCreateUser,
  changeRole as sdkChangeRole,
} from '@hc-lis/api-client';
import type { IUsersApi } from './i-users-api';
import type { UserSummary } from '../../domain/user-summary';
import type { UserRole } from '../../domain/user-session';
import type { CreateUserParams } from '../../application/i-users-port';

@Injectable()
export class SdkUsersApi implements IUsersApi {
  async listUsers(): Promise<UserSummary[]> {
    const result = await getUserList();
    const data = (result.data ?? []) as Array<{
      id?: string;
      email?: string | null;
      fullName?: string | null;
      role?: string | null;
      status?: string | null;
      createdAt?: string;
    }>;
    return data.map(item => ({
      id: item.id ?? '',
      email: item.email ?? '',
      fullName: item.fullName ?? '',
      role: (item.role ?? 'Receptionist') as UserRole,
      status: item.status ?? '',
      createdAt: item.createdAt ?? '',
    }));
  }

  async createUser(data: CreateUserParams): Promise<string> {
    const result = await sdkCreateUser({
      body: {
        email: data.email,
        fullName: data.fullName,
        birthdate: data.birthdate,
        gender: data.gender,
        role: data.role,
      },
    });
    return (result.data as { id?: string })?.id ?? '';
  }

  async changeRole(userId: string, role: UserRole): Promise<void> {
    await sdkChangeRole({
      path: { userId },
      body: { newRole: role },
    });
  }
}

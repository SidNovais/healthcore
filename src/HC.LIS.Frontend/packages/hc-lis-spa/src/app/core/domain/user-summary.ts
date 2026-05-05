import type { UserRole } from './user-session';

export interface UserSummary {
  id: string;
  email: string;
  fullName: string;
  role: UserRole;
  status: string;
  createdAt: string;
}

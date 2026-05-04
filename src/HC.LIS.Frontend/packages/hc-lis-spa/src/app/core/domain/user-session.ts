export type UserRole = 'Receptionist' | 'LabTechnician' | 'Physician' | 'ITAdmin';

export interface UserSession {
  userId: string;
  userName: string;
  role: UserRole;
}

export const ROLE_HOME_ROUTE: Record<UserRole, string> = {
  Receptionist: '/orders/new',
  LabTechnician: '/waiting-room',
  Physician: '/worklist',
  ITAdmin: '/admin/users',
};

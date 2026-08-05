import type { PhysicianStatus } from './physician-search-result';

export interface PhysicianDetails {
  id: string;
  fullName: string;
  licenceNumber: string | null;
  status: PhysicianStatus;
  registeredAt: string;
  updatedAt: string | null;
  deactivatedAt: string | null;
}

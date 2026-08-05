export type PhysicianStatus = 'Active' | 'Inactive';

export interface PhysicianSearchResult {
  id: string;
  fullName: string;
  licenceNumber: string | null;
  status: PhysicianStatus;
}

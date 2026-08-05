export interface RegisterPhysicianParams {
  fullName: string;
  licenceNumber?: string;
}

export type UpdatePhysicianParams = RegisterPhysicianParams;

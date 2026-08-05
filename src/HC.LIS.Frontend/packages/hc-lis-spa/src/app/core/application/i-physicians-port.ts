import { InjectionToken } from '@angular/core';
import type { PhysicianDetails } from '../domain/physician-details';
import type { PhysicianSearchResult } from '../domain/physician-search-result';
import type {
  RegisterPhysicianParams,
  UpdatePhysicianParams,
} from '../domain/register-physician-params';

export interface IPhysiciansPort {
  search(term: string): Promise<PhysicianSearchResult[]>;
  list(includeInactive: boolean): Promise<PhysicianSearchResult[]>;
  getDetails(id: string): Promise<PhysicianDetails>;
  register(data: RegisterPhysicianParams): Promise<string>;
  update(id: string, data: UpdatePhysicianParams): Promise<void>;
  deactivate(id: string): Promise<void>;
  reactivate(id: string): Promise<void>;
}

export const PHYSICIANS_PORT = new InjectionToken<IPhysiciansPort>('PHYSICIANS_PORT');

import { Injectable } from '@angular/core';
import {
  searchPhysicians as sdkSearchPhysicians,
  registerPhysician as sdkRegisterPhysician,
  getPhysicianDetails as sdkGetPhysicianDetails,
  updatePhysician as sdkUpdatePhysician,
  deactivatePhysician as sdkDeactivatePhysician,
  reactivatePhysician as sdkReactivatePhysician,
} from '@hc-lis/api-client';
import type { IPhysiciansPort } from '../../application/i-physicians-port';
import type { PhysicianDetails } from '../../domain/physician-details';
import type {
  PhysicianSearchResult,
  PhysicianStatus,
} from '../../domain/physician-search-result';
import type {
  RegisterPhysicianParams,
  UpdatePhysicianParams,
} from '../../domain/register-physician-params';

interface PhysicianSearchResultDto {
  id?: string;
  fullName?: string | null;
  licenceNumber?: string | null;
  status?: string | null;
}

@Injectable()
export class SdkPhysiciansAdapter implements IPhysiciansPort {
  async search(term: string): Promise<PhysicianSearchResult[]> {
    return this.query(term, false);
  }

  async list(includeInactive: boolean): Promise<PhysicianSearchResult[]> {
    const MatchEveryPhysician = '';
    return this.query(MatchEveryPhysician, includeInactive);
  }

  async getDetails(id: string): Promise<PhysicianDetails> {
    const result = await sdkGetPhysicianDetails({ path: { id } });
    const dto = result.data as
      | {
          id?: string;
          fullName?: string | null;
          licenceNumber?: string | null;
          status?: string | null;
          registeredAt?: string;
          updatedAt?: string | null;
          deactivatedAt?: string | null;
        }
      | undefined;
    return {
      id: dto?.id ?? '',
      fullName: dto?.fullName ?? '',
      licenceNumber: dto?.licenceNumber ?? null,
      status: (dto?.status ?? 'Active') as PhysicianStatus,
      registeredAt: dto?.registeredAt ?? '',
      updatedAt: dto?.updatedAt ?? null,
      deactivatedAt: dto?.deactivatedAt ?? null,
    };
  }

  async register(data: RegisterPhysicianParams): Promise<string> {
    const result = await sdkRegisterPhysician({
      body: {
        fullName: data.fullName,
        licenceNumber: data.licenceNumber,
      },
    });
    return (result.data as { id?: string })?.id ?? '';
  }

  async update(id: string, data: UpdatePhysicianParams): Promise<void> {
    await sdkUpdatePhysician({
      path: { id },
      body: {
        fullName: data.fullName,
        licenceNumber: data.licenceNumber,
      },
    });
  }

  async deactivate(id: string): Promise<void> {
    await sdkDeactivatePhysician({ path: { id } });
  }

  async reactivate(id: string): Promise<void> {
    await sdkReactivatePhysician({ path: { id } });
  }

  private async query(search: string, includeInactive: boolean): Promise<PhysicianSearchResult[]> {
    const result = await sdkSearchPhysicians({ query: { search, includeInactive } });
    const items = (result.data ?? []) as PhysicianSearchResultDto[];
    return items.map(dto => ({
      id: dto.id ?? '',
      fullName: dto.fullName ?? '',
      licenceNumber: dto.licenceNumber ?? null,
      status: (dto.status ?? 'Active') as PhysicianStatus,
    }));
  }
}
